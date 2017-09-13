using System;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>
    /// Represents a link instruction that has parameters of
    /// both the parent object and the selected property.
    /// </summary>
    /// <typeparam name="T">The type of the object being linked.</typeparam>
    /// <typeparam name="TMember">The type of the selected property.</typeparam>
    sealed class MemberLinkInstruction<T, TMember>
        : ILinkInstruction
    {
        readonly Func<T, TMember> _memberSelector;
        readonly Func<T, TMember, LinkBuilder> _linkSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberLinkInstruction{T,TProperty}"/> class.
        /// </summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function that selects a member.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type
        /// <typeparamref name="T"/> and a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public MemberLinkInstruction(
            [NotNull] string relation,
            [NotNull] Func<T, TMember> memberSelector,
            [NotNull] Func<T, TMember, LinkBuilder> linkSelector)
        {
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
            _memberSelector = memberSelector ?? throw new ArgumentNullException(nameof(memberSelector));
            _linkSelector = linkSelector ?? throw new ArgumentNullException(nameof(linkSelector));
        }

        /// <inheritdoc/>
        public string Relation { get; }

        /// <inheritdoc/>
        LinkBuilder ILinkInstruction.TransformToLinkBuilder(object main)
        {
            var value = (T)main;
            return _linkSelector(value, _memberSelector(value));
        }
    }
}
