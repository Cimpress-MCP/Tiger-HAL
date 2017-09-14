using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents a link instruction that has parameters of a single object.</summary>
    /// <typeparam name="T">The type of the object being linked.</typeparam>
    sealed class PluralSimpleLinkInstruction<T>
        : ILinkInstruction
    {
        readonly Func<T, IEnumerable<LinkBuilder>> _selector;

        /// <summary>Initializes a new instance of the <see cref="SimpleLinkInstruction{T}"/> class.</summary>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public PluralSimpleLinkInstruction([NotNull] Func<T, IEnumerable<LinkBuilder>> linkSelector)
        {
            _selector = linkSelector ?? throw new ArgumentNullException(nameof(linkSelector));
        }

        /// <inheritdoc/>
        public bool IsSingular { get; } = false;

        /// <inheritdoc/>
        IEnumerable<LinkBuilder> ILinkInstruction.TransformToLinkBuilders(object main) => _selector((T)main);
    }

    /// <summary>Represents a link instruction that has parameters of a single object.</summary>
    /// <typeparam name="T">The type of the object being linked.</typeparam>
    sealed class SimpleLinkInstruction<T>
        : ILinkInstruction
    {
        readonly Func<T, LinkBuilder> _selector;

        /// <summary>Initializes a new instance of the <see cref="SimpleLinkInstruction{T}"/> class.</summary>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public SimpleLinkInstruction([NotNull] Func<T, LinkBuilder> linkSelector)
        {
            _selector = linkSelector ?? throw new ArgumentNullException(nameof(linkSelector));
        }

        /// <inheritdoc/>
        public bool IsSingular { get; } = true;

        /// <inheritdoc/>
        IEnumerable<LinkBuilder> ILinkInstruction.TransformToLinkBuilders(object main)
        {
            yield return _selector((T)main);
        }
    }
}
