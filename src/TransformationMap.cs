using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Defines a series of transformations for a type to its HAL representation.</summary>
    [PublicAPI]
    public sealed partial class TransformationMap
    {
        /// <summary>Gets the mapping of transformation maps.</summary>
        internal IDictionary<Type, ITransformationMap> Maps { get; } =
            new Dictionary<Type, ITransformationMap>();

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="selector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public Builder<T> Self<T>([NotNull] Func<T, LinkBuilder> selector)
        {
            if (selector == null) { throw new ArgumentNullException(nameof(selector)); }

            var builder = new Builder<T>(selector);
            Maps[typeof(T)] = builder;
            return builder;
        }
    }
}
