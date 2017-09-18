using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents a link instruction that has parameters of a single object.</summary>
    /// <typeparam name="T">The type of the object being linked.</typeparam>
    sealed class LinkInstruction<T>
        : ILinkInstruction
    {
        readonly Func<T, LinkBuilder> _selector;

        /// <summary>Initializes a new instance of the <see cref="LinkInstruction{T}"/> class.</summary>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public LinkInstruction([NotNull] Func<T, LinkBuilder> linkSelector)
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
