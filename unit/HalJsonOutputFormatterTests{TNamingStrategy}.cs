using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Tiger.Hal.UnitTest.Utility;
using Xunit;
// ReSharper disable All

namespace Tiger.Hal.UnitTest
{
    /// <summary>Tests related to the <see cref="HalJsonOutputFormatter"/> class.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) })]
    public abstract class HalJsonOutputFormatterTests<TNamingStrategy>
        where TNamingStrategy : NamingStrategy, new()
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
        protected virtual void UnregisteredType_CannotWriteResult()
        {
            // arrange
            var repo = Mock.Of<IHalRepository>(r => r.CanTransform(It.IsAny<Type>()) == false);
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new TNamingStrategy()
                }
            };
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var context = Mock.Of<OutputFormatterCanWriteContext>(c => c.ObjectType == typeof(Unregistered));

            // act
            var actual = sut.CanWriteResult(context);

            // assert
            Assert.False(actual);
        }

        [Property(DisplayName = "A registered type can be written.")]
        protected virtual void RegisteredType_CanWriteResult()
        {
            // arrange
            var repo = Mock.Of<IHalRepository>(r => r.CanTransform(It.IsAny<Type>()) == true);
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new TNamingStrategy()
                }
            };
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var context = Mock.Of<OutputFormatterCanWriteContext>(c => c.ObjectType == typeof(Registered));

            // act
            var actual = sut.CanWriteResult(context);

            //assert
            Assert.True(actual);
        }

        [Property(DisplayName = "An unregistered type is serialized normally.")]
        protected virtual void UnregisteredType_NotModified(Guid id)
        {
            // arrange
            var dto = new Unregistered
            {
                Id = id
            };
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new TNamingStrategy()
                }
            };
            var sut = new HalJsonOutputFormatter(Mock.Of<IHalRepository>(), serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => writer,
                typeof(Unregistered),
                dto);

            // act
            sut.WriteResponseBodyAsync(context, Encoding.UTF8).Wait();
            var actual = JsonConvert.DeserializeObject<Unregistered>(writer.ToString(), serializerSettings);

            // assert
            Assert.Equal(id, actual.Id);
        }

        [Property(DisplayName = "A registered type creates its self link correctly.")]
        protected virtual void RegisteredType_SelfLink(Guid id, NonNull<string> route)
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
            var map = new Dictionary<Type, ITransformationMap>
            {
                [typeof(Registered)] = new TransformationMap<Registered>(r => LinkBuilder.Route(route.Get, new { r.Id }))
            };
            var repo = new HalRepository(Mock.Of<IActionContextAccessor>(), urlHelperFactory, map);
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new TNamingStrategy()
                }
            };
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => writer,
                typeof(Registered),
                dto);

            // act
            sut.WriteResponseBodyAsync(context, Encoding.UTF8).Wait();
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            // assert
            Mock.Verify(urlHelper);
            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            Assert.Equal(actual.Links.Count, 1);
            var self = Assert.Contains("self", actual.Links);
            Assert.NotNull(self);
            Assert.Equal($"https://example.invalid/registered/{id}", self.Href);
        }

        [Property(DisplayName = "A registered type creates additional links correctly.")]
        protected virtual void RegisteredType_AdditionalLink(
            Guid id,
            Guid parentId,
            UnequalNonNullPair<string> routes)
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
            var registeredMap = new TransformationMap<Registered>(r => LinkBuilder.Route(route, new { id = r.Id }))
                .Link("up", r => LinkBuilder.Route(parentRoute, new { id = r.ParentId }));
            var map = new Dictionary<Type, ITransformationMap>
            {
                [typeof(Registered)] = registeredMap
            };
            var repo = new HalRepository(Mock.Of<IActionContextAccessor>(), urlHelperFactory, map);
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new TNamingStrategy()
                }
            };
            var sut = new HalJsonOutputFormatter(repo, serializerSettings, ArrayPool<char>.Shared);
            var writer = new StringWriter();
            var context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => writer,
                typeof(Registered),
                dto);

            // act
            sut.WriteResponseBodyAsync(context, Encoding.UTF8).Wait();
            var actual = JsonConvert.DeserializeObject<HollowHal>(writer.ToString(), serializerSettings);

            // assert
            Mock.Verify(urlHelper);
            Assert.NotNull(actual);
            Assert.Empty(actual.Embedded);
            Assert.NotNull(actual.Links);
            Assert.Equal(actual.Links.Count, 2);
            var self = Assert.Contains("self", actual.Links);
            Assert.NotNull(self);
            Assert.Equal($"https://example.invalid/registered/{id}", self.Href);
            var parent = Assert.Contains("up", actual.Links);
            Assert.NotNull(parent);
            Assert.Equal($"https://example.invalid/parent/{parentId}", parent.Href);
        }
    }

    /// <summary>
    /// Tests related to the <see cref="HalJsonOutputFormatter"/> class
    /// specialized for the <see cref="DefaultNamingStrategy"/> class.
    /// </summary>
    public sealed class DefaultHalJsonOutputFormatterTests
        : HalJsonOutputFormatterTests<DefaultNamingStrategy>
    {
    }

    /// <summary>
    /// Tests related to the <see cref="HalJsonOutputFormatter"/> class
    /// specialized for the <see cref="CamelCaseNamingStrategy"/> class.
    /// </summary>
    public sealed class CamelCaseHalJsonOutputFormatterTests
        : HalJsonOutputFormatterTests<CamelCaseNamingStrategy>
    {
    }

    /// <summary>
    /// Tests related to the <see cref="HalJsonOutputFormatter"/> class
    /// specialized for the <see cref="SnakeCaseNamingStrategy"/> class.
    /// </summary>
    public sealed class SnakeCaseHalJsonOutputFormatterTests
        : HalJsonOutputFormatterTests<CamelCaseNamingStrategy>
    {
    }
}
