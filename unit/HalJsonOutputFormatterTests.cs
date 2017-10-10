﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Newtonsoft.Json;
using Test.Utility;
using Tiger.Hal;
using Xunit;
// ReSharper disable All

namespace Test
{
    /// <summary>Tests related to the <see cref="HalJsonOutputFormatter"/> class.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true, MaxTest = 0x400)]
    public static class HalJsonOutputFormatterTests
    {
        class Unregistered
        {
            public Guid Id { get; set; }
        }

        class Registered
        {
            public Guid Id { get; set; }

            [JsonIgnore]
            public Guid ParentId { get; set; }

            public string Uninteresting { get; set; }
        }

        class HollowHal
        {
            [JsonProperty("_embedded")]
            public IReadOnlyDictionary<string, object> Embedded { get; } = new Dictionary<string, object>();

            [JsonProperty("_links")]
            public IReadOnlyDictionary<string, Link> Links { get; } = new Dictionary<string, Link>();
        }

        [Property(DisplayName = "An unregistered type cannot be written.")]
        static void UnregisteredType_CannotWriteResult(JsonSerializerSettings serializerSettings)
        {
            // arrange
            var repo = Mock.Of<IHalRepository>(r => r.CanTransform(It.IsAny<Type>()) == false);
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var context = Mock.Of<OutputFormatterCanWriteContext>(c => c.ObjectType == typeof(Unregistered));

            // act
            var actual = sut.CanWriteResult(context);

            // assert
            Assert.False(actual);
        }

        [Property(DisplayName = "A registered type can be written.")]
        static void RegisteredType_CanWriteResult(JsonSerializerSettings serializerSettings)
        {
            // arrange
            var repo = Mock.Of<IHalRepository>(r => r.CanTransform(It.IsAny<Type>()) == true);
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var context = Mock.Of<OutputFormatterCanWriteContext>(c => c.ObjectType == typeof(Registered));

            // act
            var actual = sut.CanWriteResult(context);

            //assert
            Assert.True(actual);
        }

        [Property(DisplayName = "An unregistered type is serialized normally.")]
        static async Task UnregisteredType_NotModified(Guid id, JsonSerializerSettings serializerSettings)
        {
            // arrange
            var dto = new Unregistered
            {
                Id = id
            };
            var sut = new HalJsonOutputFormatter(Mock.Of<IHalRepository>(), serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => writer,
                typeof(Unregistered),
                dto);

            // act
            await sut.WriteResponseBodyAsync(context, Encoding.UTF8);
            var actual = JsonConvert.DeserializeObject<Unregistered>(writer.ToString(), serializerSettings);

            // assert
            Assert.Equal(id, actual.Id);
        }

        [Property(DisplayName = "A registered type creates its self link correctly.")]
        static async Task RegisteredType_SelfLink(
            Guid id,
            NonNull<string> route,
            JsonSerializerSettings serializerSettings)
        {
            // arrange
            var dto = new Registered
            {
                Id = id
            };
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(uh => uh.Link(route.Get, It.IsAny<object>()))
                .Returns((string _, dynamic v) => $"https://example.invalid/registered/{v.Id}")
                .Verifiable();
            var urlHelperFactory = Mock.Of<IUrlHelperFactory>(uhf =>
                uhf.GetUrlHelper(It.IsAny<ActionContext>()) == urlHelper.Object);
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = new TransformationMap.Builder<Registered>(
                    r => LinkData.Route(route.Get, new { r.Id }))
            };
            var repo = new HalRepository(map, Mock.Of<IServiceProvider>());
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => writer,
                typeof(Registered),
                dto);

            // act
            await sut.WriteResponseBodyAsync(context, Encoding.UTF8);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            // assert
            Mock.Verify(urlHelper);
            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            Assert.Equal(1, actual.Links.Count);
            var self = Assert.Contains("self", actual.Links);
            Assert.NotNull(self);
            Assert.Equal($"https://example.invalid/registered/{id}", self.Href);
        }

        [Property(DisplayName = "A registered type creates additional links correctly.")]
        static async Task RegisteredType_AdditionalLink(
            Guid id,
            Guid parentId,
            UnequalNonNullPair<string> routes,
            JsonSerializerSettings serializerSettings)
        {
            // arrange
            var dto = new Registered
            {
                Id = id,
                ParentId = parentId
            };
            var (route, parentRoute) = routes;
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(uh => uh.Link(route, It.IsAny<object>()))
                .Returns((string _, dynamic v) => $"https://example.invalid/registered/{v.id}")
                .Verifiable();
            urlHelper.Setup(uh => uh.Link(parentRoute, It.IsAny<object>()))
                .Returns((string _, dynamic v) => $"https://example.invalid/parent/{v.id}")
                .Verifiable();
            var urlHelperFactory = Mock.Of<IUrlHelperFactory>(uhf =>
                uhf.GetUrlHelper(It.IsAny<ActionContext>()) == urlHelper.Object);
            var registeredMap = new TransformationMap.Builder<Registered>(r => LinkData.Route(route, new { id = r.Id }));
            ((ITransformationMap<Registered>)registeredMap).Link("up", r => LinkData.Route(parentRoute, new { id = r.ParentId }));
            var map = new Dictionary<Type, ITransformationInstructions>
            {
                [typeof(Registered)] = registeredMap
            };
            var repo = new HalRepository(map, Mock.Of<IServiceProvider>());
            serializerSettings.Converters.Add(new LinkCollection.Converter());
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => writer,
                typeof(Registered),
                dto);

            // act
            await sut.WriteResponseBodyAsync(context, Encoding.UTF8);
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            // assert
            Mock.Verify(urlHelper);
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
    }
}