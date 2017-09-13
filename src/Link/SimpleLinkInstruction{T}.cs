using System;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents a link instruction that has parameters of a single object.</summary>
    /// <typeparam name="T">The type of the object being linked.</typeparam>
    sealed class SimpleLinkInstruction<T>
        : ILinkInstruction
    {
        readonly Func<T, LinkBuilder> _selector;

        /// <summary>Initializes a new instance of the <see cref="SimpleLinkInstruction{T}"/> class.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public SimpleLinkInstruction(
            [NotNull] string relation,
            [NotNull] Func<T, LinkBuilder> linkSelector)
        {
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
            _selector = linkSelector ?? throw new ArgumentNullException(nameof(linkSelector));
        }

        /// <inheritdoc/>
        public string Relation { get; }

        /// <inheritdoc/>
        LinkBuilder ILinkInstruction.TransformToLinkBuilder(object main) => _selector((T)main);
    }
}
