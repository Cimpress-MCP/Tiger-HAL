using System;
using System.Linq.Expressions;
using FsCheck;
using FsCheck.Xunit;
using Test.Utility;
using Tiger.Hal;
using Xunit;
using static Tiger.Hal.LinkData;

namespace Test
{
    /// <summary>
    /// Tests related to the <see cref="TransformationMapExtensions.LinkAndIgnore{T}(ITransformationMap{T}, string, Expression{Func{T, Uri}})"/> method.
    /// </summary>
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true, MaxTest = 0x400)]
    public static class ComplicatedLinkingTests
    {
        public sealed class Linker
        {
            public Uri Id { get; set; }

            public Uri Link { get; set; }
        }

        static T Id<T>(T value) => value;

        [Property(DisplayName = "Simple property selectors can be ignored.")]
        public static void Property_Ignored(NonEmptyString relation)
        {
            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            transformationMap.LinkAndIgnore(relation.Get, l => l.Link);

            ITransformationInstructions transformationInstructions = builder;
            Assert.Single(transformationInstructions.IgnoreInstructions);
        }

        [Property(DisplayName = "Selectors which are not simple property selectors cannot be ignored.")]
        // note(cosborn) Neither can I.
        public static void AnythingElse_NotIgnored(NonEmptyString relation, bool ignore)
        {
            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            transformationMap.LinkAndIgnore(relation.Get, l => Id(l.Link));

            ITransformationInstructions transformationInstructions = builder;
            Assert.Empty(transformationInstructions.IgnoreInstructions);
        }
    }
}
