using System.Collections.Generic;

namespace Tiger.Hal
{
    /// <summary>
    /// Represents the instructions necessary for transforming
    /// a value into its HAL representation.
    /// </summary>
    interface ITransformationMap
    {
        /// <summary>
        /// Gets a collection of instructions for creating link relations.
        /// </summary>
        IReadOnlyDictionary<string, ILinkInstruction> LinkInstructions { get; }

        /// <summary>
        /// Gets a collection of instructions for embedding values.
        /// </summary>
        IReadOnlyCollection<IEmbedInstruction> EmbedInstructions { get; }
    }
}
