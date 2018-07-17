using System;
using System.Collections.Generic;
using System.Globalization;
using FsCheck;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Tiger.Hal;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Test.Utility
{
    [UsedImplicitly(Members)]
    static class Generators
    {
        [NotNull]
        public static Arbitrary<AbsoluteUri> AbsoluteUri() => Arb.From(
                from scheme in Gen.Elements("http", "https")
                from hn in Arb.Generate<HostName>()
                select new UriBuilder(scheme, hn.ToString()) into ub
                select ub.Uri)
            .Convert(uri => new AbsoluteUri(uri), au => au.ToUri());

        [NotNull]
        public static Arbitrary<UnequalNonNullPair<T>> UnequalNonNullPair<T>()
            where T : class => Arb.Generate<NonNull<T>>()
            .Two().Select(nn => (left: nn.Item1.Get, right: nn.Item2.Get))
            .Where(t => !EqualityComparer<T>.Default.Equals(t.left, t.right))
            .ToArbitrary()
            .Convert(t => new UnequalNonNullPair<T>(t), unnp => (unnp.Left, unnp.Right));

        [NotNull]
        public static Arbitrary<LanguageCode> LanguageCode => Arb
            .Generate<NonNull<CultureInfo>>()
            .Select(ci => ci.Get.Name)
            .ToArbitrary()
            .Convert(lc => new LanguageCode(lc), lc => lc);

        [NotNull]
        public static Arbitrary<Link> Link => Arb.From(
            from href in Arb.Generate<AbsoluteUri>()
            from isTemplated in Arb.Generate<bool>()
            from type in Arb.Generate<string>()
            from deprecation in Arb.Generate<AbsoluteUri?>()
            from name in Arb.Generate<string>()
            from profile in Arb.Generate<AbsoluteUri?>()
            from title in Arb.Generate<string>()
            from hrefLang in Arb.Generate<LanguageCode?>()
            select new Link(href.ToString(), isTemplated, type, deprecation, name, profile, title, hrefLang));

        static readonly Gen<IContractResolver> s_contractResolver = Gen.OneOf(
            Gen.Constant<IContractResolver>(new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }),
            Gen.Constant<IContractResolver>(new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }),
            Gen.Constant<IContractResolver>(new DefaultContractResolver
            {
                NamingStrategy = new DefaultNamingStrategy()
            }));

        [NotNull]
        public static Arbitrary<JsonSerializerSettings> JsonSerializerSettings => Arb.From(
            from contractResolver in s_contractResolver
            select new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                DateParseHandling = DateParseHandling.DateTimeOffset
            });
    }
}
