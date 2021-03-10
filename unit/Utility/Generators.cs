// <copyright file="Generators.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;
using System.Globalization;
using FsCheck;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Tiger.Hal;

namespace Test.Utility
{
    static class Generators
    {
        static readonly Gen<IContractResolver> s_contractResolver = Gen.OneOf(
            Gen.Constant<IContractResolver>(new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            }),
            Gen.Constant<IContractResolver>(new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            }),
            Gen.Constant<IContractResolver>(new DefaultContractResolver
            {
                NamingStrategy = new DefaultNamingStrategy(),
            }));

        public static Arbitrary<AbsoluteUri> AbsoluteUri { get; } = Arb.From(
                from scheme in Gen.Elements("http", "https")
                from hn in Arb.Generate<HostName>()
                select new UriBuilder(scheme, hn.ToString()) into ub
                select ub.Uri)
            .Convert(uri => new AbsoluteUri(uri), au => au.ToUri());

        public static Arbitrary<LanguageCode> LanguageCode { get; } = Arb
            .Generate<NonNull<CultureInfo>>()
            .Select(ci => ci.Get.Name)
            .ToArbitrary()
            .Convert(lc => new LanguageCode(lc), lc => lc);

        public static Arbitrary<Link> Link { get; } = Arb.From(
            from href in Arb.Generate<AbsoluteUri>()
            from isTemplated in Arb.Generate<bool>()
            from type in Arb.Generate<string>()
            from deprecation in Arb.Generate<AbsoluteUri?>()
            from name in Arb.Generate<string>()
            from profile in Arb.Generate<AbsoluteUri?>()
            from title in Arb.Generate<string>()
            from hrefLang in Arb.Generate<LanguageCode?>()
            select new Link(href.ToString(), isTemplated, type, deprecation, name, profile, title, hrefLang));

        public static Arbitrary<JsonSerializerSettings> JsonSerializerSettings { get; } = Arb.From(
            from contractResolver in s_contractResolver
            select new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                DateParseHandling = DateParseHandling.DateTimeOffset,
            });

        public static Arbitrary<UnequalNonNullPair<T>> UnequalNonNullPair<T>()
            where T : class => Arb.Generate<NonNull<T>>()
            .Two().Select(nn => (Left: nn.Item1.Get, Right: nn.Item2.Get))
            .Where(t => !EqualityComparer<T>.Default.Equals(t.Left, t.Right))
            .ToArbitrary()
            .Convert(t => new UnequalNonNullPair<T>(t), unnp => (unnp.Left, unnp.Right));
    }
}
