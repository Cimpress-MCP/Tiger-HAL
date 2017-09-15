using System;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for hoisting a value in a HAL response.</summary>
    /// <typeparam name="T">The parent type of the value to hoist.</typeparam>
    /// <typeparam name="TMember">The type of the value to hoist.</typeparam>
    sealed class MemberHoistInstruction<T, TMember>
        : IHoistInstruction
    {
        readonly Func<T, TMember> _valueSelector;

        /// <summary>Initializes a new instance of the <see cref="MemberHoistInstruction{T,TMember}"/> class.</summary>
        /// <param name="memberName">The name of the member to hoist.</param>
        /// <param name="valueSelector">A function that seelcts a value to embed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="memberName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        public MemberHoistInstruction(
            [NotNull] string memberName,
            [NotNull] Func<T, TMember> valueSelector)
        {
            Name = memberName ?? throw new ArgumentNullException(nameof(memberName));
            _valueSelector = valueSelector ?? throw new ArgumentNullException(nameof(valueSelector));
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        object IHoistInstruction.GetHoistValue(object main) => _valueSelector((T)main);
    }
}
