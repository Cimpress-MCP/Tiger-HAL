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
        IReadOnlyCollection<IHoistInstruction> ITypeTransformer.Hoists => _transformationMap.HoistInstructions;

        /// <inheritdoc/>
        IReadOnlyDictionary<string, LinkCollection> ITypeTransformer.GenerateLinks(object value)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            return _transformationMap.LinkInstructions
                .SelectMany(
                    kvp => kvp.Value.TransformToLinkBuilders(value),
                    (kvp, lb) => (rel: kvp.Key, isSingular: kvp.Value.IsSingular, link: lb.Build(_urlHelper)))
                .ToLookup(kvp => (rel: kvp.rel, isSingular: kvp.isSingular), kvp => kvp.link)
                .ToDictionary(g => g.Key.rel, g => new LinkCollection(g.ToList(), g.Key.isSingular));
        }
    }
}
