using System;
using System.Collections.Generic;
using FsCheck;
using JetBrains.Annotations;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Tiger.Hal.UnitTest.Utility
{
    [UsedImplicitly(Members)]
    static class Generators
    {
        public static Arbitrary<Uri> Uri() => Arb.From(
            from hn in Arb.Generate<HostName>()
            from scheme in Gen.OneOf(Gen.Constant("http"), Gen.Constant("https"))
            select new UriBuilder(scheme, hn.Item) into ub
            select ub.Uri);

        public static Arbitrary<UnequalNonNullPair<T>> UnequalNonNullPair<T>()
            where T : class => Arb.Generate<NonNull<T>>()
            .Two().Select(t => (left: t.Item1.Get, right: t.Item2.Get))
            .Where(t => !EqualityComparer<T>.Default.Equals(t.left, t.right))
            .ToArbitrary()
            .Convert(t => new UnequalNonNullPair<T>(t), unnp => (unnp.Left, unnp.Right));

        public static Arbitrary<LanguageCode> LanguageCode() => Gen.Choose('a', 'z')
            .Select(i => (char)i).ArrayOf(2).Two()
            .Select(cs => (lang: new string(cs.Item1), nation: new string(cs.Item2)))
            .Select(t => new LanguageCode(t.lang, t.nation))
            .ToArbitrary();
    }
}
