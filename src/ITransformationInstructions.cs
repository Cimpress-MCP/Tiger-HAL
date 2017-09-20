using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>
    /// Represents the instructions necessary for transforming
    /// a value into its HAL representation.
    /// </summary>
    interface ITransformationInstructions
    {
        /// <summary>
        /// Gets a collection of instructions for creating link relations.
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, ILinkInstruction> LinkInstructions { get; }

        /// <summary>
        /// Gets a collection of instructions for embedding values.
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<IEmbedInstruction> EmbedInstructions { get; }

        /// <summary>
        /// Gets a collection of instructions for hoisting values.
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<IHoistInstruction> HoistInstructions { get; }

        /// <summary>
        /// Gets a collection of instructions for ignoring values.
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<string> IgnoreInstructions { get; }
    }
}
