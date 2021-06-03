// <copyright file="LinkTests.cs" company="Cimpress, Inc.">
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

using FsCheck;
using FsCheck.Xunit;
using Newtonsoft.Json;
using Test.Utility;
using Tiger.Hal;
using Xunit;

namespace Test
{
    /// <summary>Tests related to the <see cref="Link"/> class.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true, MaxTest = 0x400)]
    public static class LinkTests
    {
        [Property(DisplayName = "A link survives serialization.")]
        public static void Serialization_RoundTrip(Link link, JsonSerializerSettings serializerSettings)
        {
            var actual = JsonConvert.DeserializeObject<Link>(
                JsonConvert.SerializeObject(link, serializerSettings),
                serializerSettings);

            Assert.Equal(link, actual, new LinkEqualityComparer());
        }
    }
}
