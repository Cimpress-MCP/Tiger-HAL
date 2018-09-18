using System;
using System.Linq.Expressions;
using FsCheck.Xunit;
using Test.Utility;
using Tiger.Hal;
using Xunit;
using static Tiger.Hal.LinkData;

namespace Test
{
    /// <summary>
    /// Tests related to the <see cref="ITransformationMap{T}.Link(string, Expression{Func{T, Uri}}, bool)"/> method.
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

        [Property(DisplayName = "Simple property selectors are ignored by default.")]
        public static void Property_Ignored()
        {
            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            transformationMap.Link("wow", l => l.Link);

            ITransformationInstructions transformationInstructions = builder;
            Assert.Single(transformationInstructions.IgnoreInstructions);
        }

        [Property(DisplayName = "Simple property selectors can opt out of being ignored.")]
        public static void Property_NotIgnored()
        {
            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            transformationMap.Link("wow", l => l.Link, ignore: false);

            ITransformationInstructions transformationInstructions = builder;
            Assert.Empty(transformationInstructions.IgnoreInstructions);
        }

        [Property(DisplayName = "Selectors which are not simple property selectors cannot be ignored.")]
        // note(cosborn) Neither can I.
        public static void AnythingElse_NotIgnored(bool ignore)
        {
            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            transformationMap.Link("wow", l => Id(l.Link), ignore: ignore);

            ITransformationInstructions transformationInstructions = builder;
            Assert.Empty(transformationInstructions.IgnoreInstructions);
        }
    }
}
