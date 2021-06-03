// <copyright file="IgnoreTests.cs" company="Cimpress, Inc.">
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
    /// the <see cref="TransformationMapExtensions.Ignore{T, T1}(ITransformationMap{T}, Expression{Func{T, T1}})"/> method,
    /// the <see cref="TransformationMapExtensions.Ignore{T, T1, T2}(ITransformationMap{T}, Expression{Func{T, T1}}, Expression{Func{T, T2}})"/> method,
    /// the <see cref="TransformationMapExtensions.Ignore{T, T1, T2, T3}(ITransformationMap{T}, Expression{Func{T, T1}}, Expression{Func{T, T2}}, Expression{Func{T, T3}})"/> method,
    /// and the <see cref="TransformationMapExtensions.Ignore{T}(ITransformationMap{T}, Expression{Func{T, object}}[])"/> method.
    /// </summary>
    [Properties(QuietOnSuccess = true, MaxTest = 0x400)]
    public static class IgnoreTests
    {
        [Fact(DisplayName = "Simple property selectors can be ignored.")]
        public static void SimpleProperty_Ignored()
        {
            var builder = new TransformationMap.Builder<Linker>(l => Const(l.Id));
            ITransformationMap<Linker> transformationMap = builder;
            _ = transformationMap.Ignore(l => l.Link);

            ITransformationInstructions transformationInstructions = builder;
            _ = Assert.Single(transformationInstructions.IgnoreInstructions);
        }

        [Fact(DisplayName = "Selectors which are not simple property selectors cannot be ignored.")]
        /* note(cosborn) Neither can I. */
        public static void WrappedInFunction_NotIgnored()
        {
            ITransformationMap<Linker> transformationMap = new TransformationMap.Builder<Linker>(l => Const(l.Id));

            var actual = Record.Exception(() => transformationMap.Ignore(l => Id(l.Link)));

            var ae = Assert.IsType<ArgumentException>(actual);
            Assert.StartsWith(Resources.MalformedValueSelector, ae.Message, Ordinal);
        }

        [Fact(DisplayName = "Selectors which are not simple property selectors cannot be ignored.")]
        public static void DeepSelector_NotIgnored()
        {
            ITransformationMap<DeepLinker> transformationMap = new TransformationMap.Builder<DeepLinker>(l => Const(l.Linker.Id));

            var actual = Record.Exception(() => transformationMap.Ignore(l => l.Linker.Link));

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
