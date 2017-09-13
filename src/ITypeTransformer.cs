using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Transforms a value into its HAL representation.</summary>
    public interface ITypeTransformer
    {
        /// <summary>Gets a mapping of accessor to collection of link relations.</summary>
        IReadOnlyCollection<IEmbedInstruction> Embeds { get; }

        /// <summary>
        /// Generates a mapping of link relations to a collection of links
        /// for the given value.
        /// </summary>
        /// <param name="value">The value for which to generate link relations.</param>
        /// <returns>A mapping of link relations to link collection.</returns>
        /// <remarks>
        /// Remember that <see cref="LinkCollection"/> serializes to a single object
        /// if the collection is singular.
        /// </remarks>
        [NotNull]
        IReadOnlyDictionary<string, LinkCollection> GenerateLinks([NotNull] object value);
    }
}
