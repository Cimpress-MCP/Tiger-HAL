using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Tiger.Hal
{
    /// <summary>Maps types to type transformers.</summary>
    sealed class HalRepository
        : IHalRepository
    {
        readonly IActionContextAccessor _actionContextAccessor;
        readonly IUrlHelperFactory _urlHelperFactory;
        readonly IReadOnlyDictionary<Type, ITransformationMap> _transformations;

        /// <summary>Initializes a new instance of the <see cref="HalRepository"/> class.</summary>
        /// <param name="actionContextAccessor">The application's accessor for <see cref="ActionContext"/>.</param>
        /// <param name="urlHelperFactory">The application's factory for <see cref="IUrlHelper"/>.</param>
        /// <param name="transformations">A mapping of types to type transformation maps.</param>
        /// <exception cref="ArgumentNullException"><paramref name="actionContextAccessor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="urlHelperFactory"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="transformations"/> is <see langword="null"/>.</exception>
        public HalRepository(
            [NotNull] IActionContextAccessor actionContextAccessor,
            [NotNull] IUrlHelperFactory urlHelperFactory,
            [NotNull] IReadOnlyDictionary<Type, ITransformationMap> transformations)
        {
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _urlHelperFactory = urlHelperFactory ?? throw new ArgumentNullException(nameof(urlHelperFactory));
            _transformations = transformations ?? throw new ArgumentNullException(nameof(transformations));
        }

        /// <inheritdoc/>
        bool IHalRepository.CanTransform(Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return _transformations.ContainsKey(type);
        }

        /// <inheritdoc/>
        bool IHalRepository.TryGetTransformer(Type type, out ITypeTransformer transformer)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            if (!_transformations.TryGetValue(type, out var transformationMap))
            {
                transformer = default;
                return false;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            transformer = new TypeTransformer(transformationMap, urlHelper);
            return true;
        }
    }
}
