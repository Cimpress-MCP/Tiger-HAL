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
        static readonly EqualityComparer _comparer = new EqualityComparer(Ordinal);

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
                .ToLookup(kvp => (kvp.rel, kvp.isSingular), kvp => kvp.link, _comparer)
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
