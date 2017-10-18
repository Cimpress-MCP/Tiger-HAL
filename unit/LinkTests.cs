using FsCheck.Xunit;
using Newtonsoft.Json;
using Test.Utility;
using Tiger.Hal;
using Xunit;

// ReSharper disable All

namespace Test
{
    /// <summary>Tests related to the <see cref="Link"/> class.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true, MaxTest = 0x400)]
    public static class LinkTests
    {
        [Property(DisplayName = "A link survives serialization.")]
        static void Serialization_RoundTrip(Link link, JsonSerializerSettings serializerSettings)
        {
            // act
            var linkString = JsonConvert.SerializeObject(link, serializerSettings);
            var actual = JsonConvert.DeserializeObject<Link>(linkString, serializerSettings);

            // assert
            Assert.Equal(link, actual, new LinkEqualityComparer());
        }
    }
}
