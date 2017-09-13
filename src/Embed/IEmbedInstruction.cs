using System;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /* note(cosborn)
     * Embed Instructions are complex to support the scenario
     * of embedding properties on an object that serializes
     * to an array. Json.NET won't let me get a valid
     * JsonObjectContract for a thing that "should" have a
     * JsonArrayContract, so we have to introduce a little
     * of the ol' razzle-dazzle.
     */

    /// <summary>Represents an instruction for creating an embed in a HAL response.</summary>
    public interface IEmbedInstruction
    {
        /// <summary>Gets the name of the link relation to establish.</summary>
        [NotNull]
        string Relation { get; }

        /// <summary>Gets the path into the object to select the value to embed.</summary>
        [NotNull]
        object Index { get; }

        /// <summary>Gets the type of the value to embed.</summary>
        [NotNull]
        Type Type { get; }

        /// <summary>Retrieves the value to be embedded from the given main object.</summary>
        /// <param name="main">The main object.</param>
        /// <returns>The value to be embedded.</returns>
        [NotNull]
        object GetEmbeddedValue([NotNull] object main);
    }
}
