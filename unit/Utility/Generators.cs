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
        static readonly char[] s_alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        [NotNull]
        public static Arbitrary<AbsoluteUri> Uri() => Arb.From(
            from scheme in Gen.OneOf(Gen.Constant("http"), Gen.Constant("https"))
            from hn in Arb.Generate<HostName>()
            select new UriBuilder(scheme, hn.Item) into ub
            select ub.Uri)
            .Convert(uri => new AbsoluteUri(uri), au => au);

        [NotNull]
        public static Arbitrary<UnequalNonNullPair<T>> UnequalNonNullPair<T>()
            where T : class => Arb.Generate<NonNull<T>>()
            .Two().Select(nn => (left: nn.Item1.Get, right: nn.Item2.Get))
            .Where(t => !EqualityComparer<T>.Default.Equals(t.left, t.right))
            .ToArbitrary()
            .Convert(t => new UnequalNonNullPair<T>(t), unnp => (unnp.Left, unnp.Right));

        [NotNull]
        public static Arbitrary<LanguageCode> LanguageCode() => Arb.Generate<NonNull<CultureInfo>>()
            .Select(ci => ci.Get.Name)
            .ToArbitrary()
            .Convert(lc => new LanguageCode(lc), lc => lc);

        [NotNull]
        public static Arbitrary<Link> Link() => Arb.From(
            from href in Arb.Generate<AbsoluteUri>()
            from isTemplated in Arb.Generate<bool>()
            from type in Arb.Generate<string>()
            from deprecation in Arb.Generate<AbsoluteUri?>()
            from name in Arb.Generate<string>()
            from profile in Arb.Generate<AbsoluteUri?>()
            from title in Arb.Generate<string>()
            from hrefLang in Arb.Generate<LanguageCode?>()
            select new Link(href.Get.AbsoluteUri, isTemplated, type, deprecation, name, profile, title, hrefLang));

        [NotNull]
        public static Arbitrary<NamingStrategy> NamingStrategy() => Gen.OneOf(
                Gen.Fresh(() => (NamingStrategy)new CamelCaseNamingStrategy()),
                Gen.Fresh(() => (NamingStrategy)new SnakeCaseNamingStrategy()),
                Gen.Fresh(() => (NamingStrategy)new DefaultNamingStrategy()))
            .ToArbitrary();

        [NotNull]
        public static Arbitrary<IContractResolver> ContractResolver() => Arb.From(
            from namingStrategy in Arb.Generate<NamingStrategy>()
            select (IContractResolver)new DefaultContractResolver
            {
                NamingStrategy = namingStrategy
            });

        [NotNull]
        public static Arbitrary<JsonSerializerSettings> JsonSerializerSettings() => Arb.From(
            from contractResolver in Arb.Generate<IContractResolver>()
            select new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Formatting = Formatting.Indented
            });
    }
}
