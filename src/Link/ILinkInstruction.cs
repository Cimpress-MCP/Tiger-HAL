using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for creating a link relation in a HAL response.</summary>
    interface ILinkInstruction
    {
        /// <summary>
        /// Gets a value indicating whether this link instruction represents a single value
        /// when it produces a singleton collection from <see cref="TransformToLinkBuilders"/>.
        /// </summary>
        bool IsSingular { get; }

        /// <summary>
        /// Transforms this instance into a collection of instances of <see cref="LinkBuilder"/>.
        /// </summary>
        /// <param name="main">The main object from which to create a link collection.</param>
        /// <returns>A collection of instances of <see cref="LinkBuilder"/>.</returns>
        [NotNull, ItemNotNull]
        IEnumerable<LinkBuilder> TransformToLinkBuilders([NotNull] object main);
    }
}
