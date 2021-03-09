// <copyright file="HalJsonOutputFormatterTests.cs" company="Cimpress, Inc.">
//   Copyright 2020 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Test.Utility;
using Tiger.Hal;
using Xunit;
using static System.UriKind;
using static Tiger.Hal.LinkData;

namespace Test
{
    /// <summary>Tests related to the <see cref="HalJsonOutputFormatter"/> class.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true, MaxTest = 0x400)]
    public static class HalJsonOutputFormatterTests
    {
        [Property(DisplayName = "An unregistered type cannot be written.")]
        public static void UnregisteredType_CannotWriteResult(JsonSerializerSettings serializerSettings)
        {
            var repo = new HalRepository(ImmutableDictionary<Type, ITransformationInstructions>.Empty, new ServiceCollection().BuildServiceProvider());
            var sut = new HalJsonOutputFormatter(serializerSettings, ArrayPool<char>.Shared, new MvcOptions(), repo);
            var context = new OutputFormatterWriteContext(new DefaultHttpContext(), (_, _) => new StreamWriter(Stream.Null), typeof(Unregistered), new Unregistered());

            Assert.False(sut.CanWriteResult(context));
        }

        [Property(DisplayName = "A registered type can be written.")]
        public static void RegisteredType_CanWriteResult(JsonSerializerSettings serializerSettings)
        {
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = new TransformationMap.Builder<Registered>(_ => Const(new Uri("about:blank", Absolute))),
            };
            var repo = new HalRepository(map, new ServiceCollection().BuildServiceProvider());
            var sut = new HalJsonOutputFormatter(serializerSettings, ArrayPool<char>.Shared, new MvcOptions(), repo);
            var context = new OutputFormatterWriteContext(new DefaultHttpContext(), (_, _) => new StreamWriter(Stream.Null), typeof(Registered), new Registered());

            Assert.True(sut.CanWriteResult(context));
        }

        [Property(DisplayName = "An unregistered type is serialized normally.")]
        public static async Task UnregisteredType_NotModified(Guid id, JsonSerializerSettings serializerSettings)
        {
            var dto = new Unregistered
            {
                Id = id,
            };
            using var svc = new ServiceContainer();
            var repo = new HalRepository(ImmutableDictionary<Type, ITransformationInstructions>.Empty, svc);
            var sut = new HalJsonOutputFormatter(serializerSettings, ArrayPool<char>.Shared, new MvcOptions(), repo);
            await using var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, _) => writer,
                typeof(Unregistered),
                dto);

            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<Unregistered>(writer.ToString(), serializerSettings);

            Assert.Equal(id, actual.Id);
        }

        [Property(DisplayName = "A registered type creates its self link correctly.")]
        public static async Task RegisteredType_SelfLink(Guid id, JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings is null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            var dto = new Registered
            {
                Id = id,
            };
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = new TransformationMap.Builder<Registered>(
                    r => Const(new Uri($"https://example.invalid/registered/{r.Id}"))),
            };
            await using var serviceProvider = new ServiceCollection()
                .AddScoped<ILinkBuilder<Constant>, LinkBuilder.Constant>()
                .BuildServiceProvider();
            var repo = new HalRepository(map, serviceProvider);
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(serializerSettings, ArrayPool<char>.Shared, new MvcOptions(), repo);
            using var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, _) => writer,
                typeof(Registered),
                dto);

            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            Assert.Equal(1, actual.Links.Count);
            var self = Assert.Contains(Relations.Self, actual.Links);
            Assert.NotNull(self);
            Assert.Equal($"https://example.invalid/registered/{id}", self.Href);
        }

        [Property(DisplayName = "A registered type creates additional links correctly.")]
        public static async Task RegisteredType_AdditionalLink(
            Guid id,
            Guid parentId,
            UnequalNonNullPair<NonEmptyString> routes,
            JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings is null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            var dto = new Registered
            {
                Id = id,
                ParentId = parentId,
            };
            var (route, parentRoute) = routes;
            var builder = new TransformationMap.Builder<Registered>(r => Const(new Uri($"https://example.invalid/registered/{r.Id}")));
            ITransformationMap<Registered> transformationMap = builder;
            _ = transformationMap.Link("up", r => Const(new Uri($"https://example.invalid/parent/{r.ParentId}")));
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = builder,
            };
            await using var serviceProvider = new ServiceCollection()
                .AddScoped<ILinkBuilder<Constant>, LinkBuilder.Constant>()
                .BuildServiceProvider();
            var repo = new HalRepository(map, serviceProvider);
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(serializerSettings, ArrayPool<char>.Shared, new MvcOptions(), repo);
            using var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, _) => writer,
                typeof(Registered),
                dto);

            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            Assert.Equal(2, actual.Links.Count);
            var self = Assert.Contains("self", actual.Links);
            Assert.NotNull(self);
            Assert.Equal($"https://example.invalid/registered/{id}", self.Href);
            var parent = Assert.Contains("up", actual.Links);
            Assert.NotNull(parent);
            Assert.Equal($"https://example.invalid/parent/{parentId}", parent.Href);
        }

        [Property(DisplayName = "A null link does not serialize.")]
        public static async Task RegisteredType_NullLink(
            Guid id,
            Guid parentId,
            UnequalNonNullPair<NonEmptyString> routes,
            JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings is null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            var dto = new Registered
            {
                Id = id,
                ParentId = parentId,
            };
            var (route, parentRoute) = routes;
            var builder = new TransformationMap.Builder<Registered>(r => Const(new Uri($"https://example.invalid/registered/{r.Id}")));
            ITransformationMap<Registered> transformationMap = builder;
            _ = transformationMap.Link("up", _ => (ILinkData?)null);
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = builder,
            };
            await using var serviceProvider = new ServiceCollection()
                .AddScoped<ILinkBuilder<Constant>, LinkBuilder.Constant>()
                .BuildServiceProvider();
            var repo = new HalRepository(map, serviceProvider);
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(serializerSettings, ArrayPool<char>.Shared, new MvcOptions(), repo);
            using var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, _) => writer,
                typeof(Registered),
                dto);

            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            var (rel, link) = Assert.Single(actual.Links);
            Assert.Equal("self", rel);
            Assert.Equal($"https://example.invalid/registered/{id}", link.Href);
        }

        sealed class Unregistered
        {
            public Guid Id { get; set; }
        }

        sealed class Registered
        {
            public Guid Id { get; set; }

            [JsonIgnore]
            public Guid ParentId { get; set; }

            public string? Uninteresting { get; set; }
        }

        [SuppressMessage("Microsoft.Style", "CA1812", Justification = "Deserialization target.")]
        sealed class HollowHal
        {
            [JsonProperty("_embedded")]
            public IReadOnlyDictionary<string, object> Embedded { get; } = new Dictionary<string, object>();

            [JsonProperty("_links")]
            public IReadOnlyDictionary<string, Link> Links { get; } = new Dictionary<string, Link>();
        }
    }
}
