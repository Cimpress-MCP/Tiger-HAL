using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for creating a link relation in a HAL response.</summary>
    interface ILinkInstruction
    {
        /// <summary>Gets the name of the link relation to establish.</summary>
        [NotNull]
        string Relation { get; }

        /// <summary>Transforms this instance into an instance of <see cref="LinkBuilder"/>.</summary>
        /// <param name="main">The main object from which to create a link.</param>
        /// <returns>A instance of <see cref="LinkBuilder"/>.</returns>
        [NotNull]
        LinkBuilder TransformToLinkBuilder([NotNull] object main);
    }
}
