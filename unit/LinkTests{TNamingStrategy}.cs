using System;
using FsCheck;
using FsCheck.Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Test.Utility;
using Tiger.Hal;
using Xunit;
// ReSharper disable All

namespace Test
{
    /// <summary>Tests related to the <see cref="Link"/> class.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) })]
    public abstract class LinkTests<TNamingStrategy>
        where TNamingStrategy : NamingStrategy, new()
    {
        [Property(DisplayName = "A link survives serialization.")]
        protected void Serialization_RoundTrip(
            NonNull<Uri> href,
            bool isTemplated,
            string type,
            Uri deprecation,
            string name,
            Uri profile,
            string title,
            LanguageCode hrefLang)
        {
            // arrange
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new TNamingStrategy()
                }
            };
            var link = new Link(href.Get.AbsoluteUri, isTemplated, type, deprecation, name, profile, title, hrefLang);

            // act
            var linkString = JsonConvert.SerializeObject(link, jsonSerializerSettings);
            var actual = JsonConvert.DeserializeObject<Link>(linkString, jsonSerializerSettings);

            // assert
            Assert.Equal(link, actual, new LinkEqualityComparer());
        }
    }

    /// <summary>
    /// Tests related to the <see cref="Link"/> class
    /// specialized for the <see cref="DefaultNamingStrategy"/> class.
    /// </summary>
    public sealed class DefaultLinkTests
        : LinkTests<DefaultNamingStrategy>
    {
    }

    /// <summary>
    /// Tests related to the <see cref="Link"/> class
    /// specialized for the <see cref="CamelCaseNamingStrategy"/> class.
    /// </summary>
    public sealed class CamelCaseLinkTests
        : LinkTests<CamelCaseNamingStrategy>
    {
    }

    /// <summary>
    /// Tests related to the <see cref="Link"/> class
    /// specialized for the <see cref="SnakeCaseNamingStrategy"/> class.
    /// </summary>
    public sealed class SnakeCaseLinkTests
        : LinkTests<SnakeCaseNamingStrategy>
    {
    }
}
