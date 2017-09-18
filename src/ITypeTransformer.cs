using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Transforms a value into its HAL representation.</summary>
    public interface ITypeTransformer
    {
        /// <summary>Gets a mapping of accessor to embeddable value.</summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<IEmbedInstruction> Embeds { get; }

        /// <summary>Gets a mapping of accessor to hoistable value.</summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<IHoistInstruction> Hoists { get; }

        /// <summary>Gets a collection of ignorable properties.</summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<string> Ignores { get; }

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
