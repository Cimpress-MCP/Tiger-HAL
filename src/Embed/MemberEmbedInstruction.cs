using System;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for embedding a value in a HAL response.</summary>
    /// <typeparam name="T">The parent type of the value to embed.</typeparam>
    /// <typeparam name="TSelected">The type of the value to embed.</typeparam>
    sealed class MemberEmbedInstruction<T, TSelected>
        : IEmbedInstruction
    {
        readonly string _memberName;
        readonly Func<T, TSelected> _valueSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberEmbedInstruction{T,TSelected}"/> class.
        /// </summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberName">The path into the object to select the value to embed.</param>
        /// <param name="valueSelector">A function that selects a value to embed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        public MemberEmbedInstruction(
            [NotNull] string relation,
            [NotNull] string memberName,
            [NotNull] Func<T, TSelected> valueSelector)
        {
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
            _memberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
            _valueSelector = valueSelector ?? throw new ArgumentNullException(nameof(valueSelector));
        }

        /// <inheritdoc/>
        public string Relation { get; }

        /// <inheritdoc/>
        object IEmbedInstruction.Index => _memberName;

        /// <inheritdoc/>
        public Type Type => typeof(TSelected);

        /// <inheritdoc/>
        public object GetEmbeddedValue(object main) => _valueSelector((T)main);
    }
}
