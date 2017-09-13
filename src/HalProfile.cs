using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Defines the HAL+JSON transformations for an application.</summary>
    [PublicAPI]
    public abstract class HalProfile
    {
        /// <summary>Gets the mapping of transformation maps.</summary>
        internal IDictionary<Type, ITransformationMap> TransformationMaps { get; } =
            new Dictionary<Type, ITransformationMap>();

        /// <summary>
        /// Creates a transformation from a value to its HAL version.
        /// </summary>
        /// <typeparam name="T">
        /// The type for which the transformation is being defined.
        /// </typeparam>
        /// <param name="selfSelector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// A transformation map through which further transformations
        /// can be defined.
        /// </returns>
        [NotNull]
        protected TransformationMap<T> CreateTransformation<T>(
            [NotNull] Func<T, LinkBuilder> selfSelector)
        {
            if (selfSelector == null) { throw new ArgumentNullException(nameof(selfSelector)); }

            var map = new TransformationMap<T>(selfSelector);
            TransformationMaps[typeof(T)] = map;
            return map;
        }
    }
}
