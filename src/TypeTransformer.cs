// <copyright file="TypeTransformer.cs" company="Cimpress, Inc.">
//   Copyright 2017 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
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
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using static System.StringComparison;

namespace Tiger.Hal
{
    /// <summary>Transforms a value into its HAL representation.</summary>
    sealed class TypeTransformer
        : ITypeTransformer
    {
        static readonly EqualityComparer s_comparer = new EqualityComparer(Ordinal);

        readonly ITransformationInstructions _transformationInstructions;
        readonly IUrlHelper _urlHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeTransformer"/> class.
        /// </summary>
        /// <param name="transformationInstructions">The transformation map for a type.</param>
        /// <param name="urlHelper">The application's URL generator.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transformationInstructions"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="urlHelper"/> is <see langword="null"/>.</exception>
        public TypeTransformer(
            [NotNull] ITransformationInstructions transformationInstructions,
            [NotNull] IUrlHelper urlHelper)
        {
            _transformationInstructions = transformationInstructions ?? throw new ArgumentNullException(nameof(transformationInstructions));
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
        }

        /// <inheritdoc/>
        IReadOnlyCollection<IEmbedInstruction> ITypeTransformer.Embeds => _transformationInstructions.EmbedInstructions;

        /// <inheritdoc/>
        IReadOnlyCollection<IHoistInstruction> ITypeTransformer.Hoists => _transformationInstructions.HoistInstructions;

        /// <inheritdoc/>
        IReadOnlyCollection<string> ITypeTransformer.Ignores => _transformationInstructions.IgnoreInstructions;

        /// <inheritdoc/>
        IReadOnlyDictionary<string, LinkCollection> ITypeTransformer.GenerateLinks(object value)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            return _transformationInstructions.LinkInstructions
                .SelectMany(
                    kvp => kvp.Value.TransformToLinkBuilders(value),
                    (kvp, lb) => (rel: kvp.Key, isSingular: kvp.Value.IsSingular, link: lb.Build(_urlHelper)))
                .ToLookup(kvp => (kvp.rel, kvp.isSingular), kvp => kvp.link, s_comparer)
                .ToDictionary(g => g.Key.rel, g => new LinkCollection(g.ToList(), g.Key.isSingular));
        }

        /// <inheritdoc/>
        sealed class EqualityComparer
            : IEqualityComparer<(string rel, bool isSingular)>
        {
            readonly StringComparison _comparison;

            /// <summary>Initializes a new instance of the <see cref="EqualityComparer"/> class.</summary>
            /// <param name="comparison">The type of string comparison to use for "rel".</param>
            public EqualityComparer(StringComparison comparison)
            {
                _comparison = comparison;
            }

            /// <inheritdoc/>
            bool IEqualityComparer<(string rel, bool isSingular)>.Equals(
                (string rel, bool isSingular) x,
                (string rel, bool isSingular) y) => string.Equals(x.rel, y.rel, _comparison) &&
                                                    x.isSingular == y.isSingular;

            /// <inheritdoc/>
            int IEqualityComparer<(string rel, bool isSingular)>.GetHashCode((string rel, bool isSingular) obj) =>
                obj.GetHashCode();
        }
    }
}
