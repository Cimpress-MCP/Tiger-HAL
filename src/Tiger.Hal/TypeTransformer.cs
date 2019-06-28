// <copyright file="TypeTransformer.cs" company="Cimpress, Inc.">
//   Copyright 2018 Cimpress, Inc.
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
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using static System.StringComparison;

namespace Tiger.Hal
{
    /// <summary>Transforms a value into its HAL representation.</summary>
    sealed partial class TypeTransformer
        : ITypeTransformer
    {
        static readonly KeyEqualityComparer s_comparer = new KeyEqualityComparer(Ordinal);

        readonly ITransformationInstructions _transformationInstructions;
        readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeTransformer"/> class.
        /// </summary>
        /// <param name="transformationInstructions">The transformation map for a type.</param>
        /// <param name="serviceProvider">The application's serviceProvider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transformationInstructions"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
        public TypeTransformer(
            [NotNull] ITransformationInstructions transformationInstructions,
            [NotNull] IServiceProvider serviceProvider)
        {
            _transformationInstructions = transformationInstructions ?? throw new ArgumentNullException(nameof(transformationInstructions));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        IReadOnlyCollection<IEmbedInstruction> ITypeTransformer.Embeds => _transformationInstructions.EmbedInstructions;

        /// <inheritdoc/>
        IReadOnlyCollection<IHoistInstruction> ITypeTransformer.Hoists => _transformationInstructions.HoistInstructions;

        /// <inheritdoc/>
        IReadOnlyCollection<string> ITypeTransformer.Ignores => _transformationInstructions.IgnoreInstructions;

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">A builder for the provided <see cref="ILinkData"/> could not be resolved.</exception>
        IReadOnlyDictionary<string, LinkCollection> ITypeTransformer.GenerateLinks(object value)
        {
            if (value is null) { throw new ArgumentNullException(nameof(value)); }

            var dict = _transformationInstructions
                .LinkInstructions
                .SelectMany(
                    kvp => kvp.Value.TransformToLinkData(value),
                    (kvp, ld) => (rel: kvp.Key, isSingular: kvp.Value.IsSingular(value), link: Build(ld)))
                .ToLookup(kvp => (kvp.rel, kvp.isSingular), kvp => kvp.link, s_comparer)
                .ToImmutableDictionary(g => g.Key.rel, g => new LinkCollection(g.ToList(), g.Key.isSingular));
            var allowedEmpty = _transformationInstructions
                .LinkInstructions
                .Where(kvp => !kvp.Value.IsSingular(value))
                .Select(kvp => kvp.Key)
                .Where(k => !dict.ContainsKey(k))
                .Select(k => new KeyValuePair<string, LinkCollection>(k, new LinkCollection(ImmutableArray<Link>.Empty, isSingular: false)));
            return dict.AddRange(allowedEmpty);
        }

        /// <summary>Builds a link from the provided link data.</summary>
        /// <param name="linkData">The data from which to build a link.</param>
        /// <returns>A link.</returns>
        /// <exception cref="InvalidOperationException">A builder for the provided <see cref="ILinkData"/> could not be resolved.</exception>
        [NotNull]
        Link Build([NotNull] ILinkData linkData)
        {
            var dataType = linkData.GetType();
            var builderType = typeof(ILinkBuilder<>).MakeGenericType(dataType);
            var buildMethod = builderType.GetMethod(nameof(ILinkBuilder<ILinkData>.Build), new[] { dataType });

            object builder;
            using (var scope = _serviceProvider.CreateScope())
            {
                builder = scope.ServiceProvider.GetRequiredService(builderType);
            }

            return (Link)buildMethod.Invoke(builder, new object[] { linkData });
        }
    }
}
