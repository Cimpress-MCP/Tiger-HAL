using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
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
        public class Unregistered
        {
            public Guid Id { get; set; }
        }

        public class Registered
        {
            public Guid Id { get; set; }

            [JsonIgnore]
            public Guid ParentId { get; set; }

            public string Uninteresting { get; set; }
        }

        public class HollowHal
        {
            [JsonProperty("_embedded")]
            public IReadOnlyDictionary<string, object> Embedded { get; } = new Dictionary<string, object>();

            [JsonProperty("_links")]
            public IReadOnlyDictionary<string, Link> Links { get; } = new Dictionary<string, Link>();
        }

        [Property(DisplayName = "An unregistered type cannot be written.")]
        public static void UnregisteredType_CannotWriteResult(JsonSerializerSettings serializerSettings)
        {
            // arrange
            var repo = new HalRepository(ImmutableDictionary<Type, ITransformationInstructions>.Empty, new ServiceCollection().BuildServiceProvider());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var context = new OutputFormatterWriteContext(new DefaultHttpContext(), (_, __) => new StreamWriter(Stream.Null), typeof(Unregistered), null);

            // act
            var actual = sut.CanWriteResult(context);

            // assert
            Assert.False(actual);
        }

        [Property(DisplayName = "A registered type can be written.")]
        public static void RegisteredType_CanWriteResult(JsonSerializerSettings serializerSettings)
        {
            // arrange
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = new TransformationMap.Builder<Registered>(_ => Const(new Uri("about:blank", Absolute)))
            };
            var repo = new HalRepository(map, new ServiceCollection().BuildServiceProvider());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var context = new OutputFormatterWriteContext(new DefaultHttpContext(), (_, __) => new StreamWriter(Stream.Null), typeof(Registered), null);

            // act
            var actual = sut.CanWriteResult(context);

            //assert
            Assert.True(actual);
        }

        [Property(DisplayName = "An unregistered type is serialized normally.")]
        public static async Task UnregisteredType_NotModified(Guid id, JsonSerializerSettings serializerSettings)
        {
            // arrange
            var dto = new Unregistered
            {
                Id = id
            };
            var repo = new HalRepository(ImmutableDictionary<Type, ITransformationInstructions>.Empty, new ServiceContainer());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, __) => writer,
                typeof(Unregistered),
                dto);

            // act
            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<Unregistered>(writer.ToString(), serializerSettings);

            // assert
            Assert.Equal(id, actual.Id);
        }

        [Property(DisplayName = "A registered type creates its self link correctly.")]
        public static async Task RegisteredType_SelfLink(Guid id, JsonSerializerSettings serializerSettings)
        {
            // arrange
            var dto = new Registered
            {
                Id = id
            };
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = new TransformationMap.Builder<Registered>(
                    r => Const(new Uri($"https://example.invalid/registered/{r.Id}")))
            };
            var serviceProvider = new ServiceCollection()
                .AddScoped<ILinkBuilder<Constant>, LinkBuilder.Constant>()
                .BuildServiceProvider();
            var repo = new HalRepository(map, serviceProvider);
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, __) => writer,
                typeof(Registered),
                dto);

            // act
            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            // assert
            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            Assert.Equal(1, actual.Links.Count);
            var self = Assert.Contains("self", actual.Links);
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
            // arrange
            var dto = new Registered
            {
                Id = id,
                ParentId = parentId
            };
            var (route, parentRoute) = routes;
            var builder = new TransformationMap.Builder<Registered>(r => Const(new Uri($"https://example.invalid/registered/{r.Id}")));
            ITransformationMap<Registered> transformationMap = builder;
            transformationMap.Link("up", r => Const(new Uri($"https://example.invalid/parent/{r.ParentId}")));
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = builder
            };
            var serviceProvider = new ServiceCollection()
                .AddScoped<ILinkBuilder<Constant>, LinkBuilder.Constant>()
                .BuildServiceProvider();
            var repo = new HalRepository(map, serviceProvider);
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, __) => writer,
                typeof(Registered),
                dto);

            // act
            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            // assert
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
            // arrange
            var dto = new Registered
            {
                Id = id,
                ParentId = parentId
            };
            var (route, parentRoute) = routes;
            var builder = new TransformationMap.Builder<Registered>(r => Const(new Uri($"https://example.invalid/registered/{r.Id}")));
            ITransformationMap<Registered> transformationMap = builder;
            transformationMap.Link("up", _ => null);
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = builder
            };
            var serviceProvider = new ServiceCollection()
                .AddScoped<ILinkBuilder<Constant>, LinkBuilder.Constant>()
                .BuildServiceProvider();
            var repo = new HalRepository(map, serviceProvider);
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (_, __) => writer,
                typeof(Registered),
                dto);

            // act
            await sut.WriteResponseBodyAsync(context, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)).ConfigureAwait(false);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            // assert
            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            var (rel, link) = Assert.Single(actual.Links);
            Assert.Equal("self", rel);
            Assert.Equal($"https://example.invalid/registered/{id}", link.Href);
        }
    }
}
