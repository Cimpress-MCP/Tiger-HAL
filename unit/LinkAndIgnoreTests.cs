using System;
using System.Linq.Expressions;
using FsCheck;
using FsCheck.Xunit;
using Tiger.Hal;
using Tiger.Hal.Properties;
using Xunit;
using static System.StringComparison;
using static Tiger.Hal.LinkData;

namespace Test
{
    /// <summary>
    /// Tests related to
    /// the <see cref="TransformationMapExtensions.LinkAndIgnore{T}(ITransformationMap{T}, string, Expression{Func{T, Uri}})"/> method
    /// and the <see cref="TransformationMapExtensions.LinkAndIgnore{T}(ITransformationMap{T}, Uri, Expression{Func{T, Uri}})"/> method.
    /// </summary>
    [Properties(QuietOnSuccess = true, MaxTest = 0x400)]
    public static class LinkAndIgnoreTests
    {
        public sealed class Linker
        {
            public Uri Id { get; set; }

            public Uri Link { get; set; }
        }

        public sealed class DeepLinker
        {
            public Linker Linker { get; set; }
        }

        static T Id<T>(T value) => value;

        [Property(DisplayName = "Simple property selectors can be linked and ignored.")]
        public static void SimpleProperty_LinkAndIgnored(NonEmptyString relation)
        {
            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            transformationMap.LinkAndIgnore(relation.Get, l => l.Link);

            ITransformationInstructions transformationInstructions = builder;
            Assert.Single(transformationInstructions.IgnoreInstructions);
        }

        [Property(DisplayName = "Selectors which are not simple property selectors cannot be linked and ignored.")]
        public static void WrappedInFunction_NotLinkAndIgnored(NonEmptyString relation)
        {
            ITransformationMap<Linker> transformationMap = new TransformationMap.Builder<Linker>(l => Const(l.Id));

            var actual = Record.Exception(() => transformationMap.LinkAndIgnore(relation.Get, l => Id(l.Link)));

            var ae = Assert.IsType<ArgumentException>(actual);
            Assert.StartsWith(Resources.MalformedValueSelector, ae.Message, Ordinal);
        }

        [Property(DisplayName = "Selectors which are not simple property selectors cannot be linked and ignored.")]
        public static void DeepSelector_NotLinkAndIgnored(NonEmptyString relation)
        {
            ITransformationMap<DeepLinker> transformationMap = new TransformationMap.Builder<DeepLinker>(l => Const(l.Linker.Id));

            var actual = Record.Exception(() => transformationMap.LinkAndIgnore(relation.Get, l => l.Linker.Link));

            var ae = Assert.IsType<ArgumentException>(actual);
            Assert.StartsWith(Resources.MalformedValueSelector, ae.Message, Ordinal);
        }
    }
}
