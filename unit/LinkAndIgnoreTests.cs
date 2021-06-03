// <copyright file="LinkAndIgnoreTests.cs" company="Cimpress, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FsCheck;
using FsCheck.Xunit;
using Tiger.Hal;
using Tiger.Hal.Properties;
using Xunit;
using static System.StringComparison;
using static System.UriKind;
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
        [Property(DisplayName = "Simple property selectors can be linked and ignored.")]
        public static void SimpleProperty_LinkAndIgnored(NonEmptyString relation)
        {
            if (relation is null)
            {
                throw new ArgumentNullException(nameof(relation));
            }

            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            _ = transformationMap.LinkAndIgnore(relation.Get, l => l.Link);

            ITransformationInstructions transformationInstructions = builder;
            _ = Assert.Single(transformationInstructions.IgnoreInstructions);
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

        static T Id<T>(T value) => value;

        sealed class Linker
        {
            public Uri Id { get; set; } = new Uri("about:blank", Absolute);

            public Uri Link { get; set; } = new Uri("about:blank", Absolute);
        }

        [SuppressMessage("Microsoft.Style", "CA1812", Justification = "Deserialization target.")]
        sealed class DeepLinker
        {
            public Linker Linker { get; set; } = new Linker();
        }
    }
}
