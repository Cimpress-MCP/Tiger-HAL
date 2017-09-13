using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Tiger.Hal
{
    /// <summary>Transforms a value into its HAL representation.</summary>
    sealed class TypeTransformer
        : ITypeTransformer
    {
        readonly ITransformationMap _transformationMap;
        readonly IUrlHelper _urlHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeTransformer"/> class.
        /// </summary>
        /// <param name="transformationMap">The transformation map for a type.</param>
        /// <param name="urlHelper">The application's URL generator.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="urlHelper"/> is <see langword="null"/>.</exception>
        public TypeTransformer(
            [NotNull] ITransformationMap transformationMap,
            [NotNull] IUrlHelper urlHelper)
        {
            _transformationMap = transformationMap ?? throw new ArgumentNullException(nameof(transformationMap));
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
        }

        /// <inheritdoc/>
        IReadOnlyCollection<IEmbedInstruction> ITypeTransformer.Embeds => _transformationMap.EmbedInstructions;

        /// <inheritdoc/>
        IReadOnlyDictionary<string, LinkCollection> ITypeTransformer.GenerateLinks(object value)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            return _transformationMap.LinkInstructions
                .Select(li => (rel: li.Relation, builder: li.TransformToLinkBuilder(value)))
                .Select(kvp => (rel: kvp.rel, link: kvp.builder.Build(_urlHelper)))
                .ToLookup(kvp => kvp.rel, kvp => kvp.link)
                .ToDictionary(g => g.Key, kvp => new LinkCollection(kvp.ToList()));
        }
    }
}
