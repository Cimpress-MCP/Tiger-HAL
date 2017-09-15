using JetBrains.Annotations;

namespace Test.Utility
{
    /// <summary>Generates a pair of values which are non-<see langword="null"/> and unequal.</summary>
    /// <typeparam name="T">The member type.</typeparam>
    public struct UnequalNonNullPair<T>
        where T : class
    {
        readonly (T left, T right) _values;

        /// <summary>Initializes a new instance of the <see cref="UnequalNonNullPair{T}"/> struct.</summary>
        /// <param name="values">The values of the pair.</param>
        public UnequalNonNullPair((T left, T right) values)
        {
            _values = values;
        }

        /// <summary>Gets the left value.</summary>
        public T Left => _values.left;

        /// <summary>Gets the right value.</summary>
        public T Right => _values.right;

        /// <inheritdoc/>
        [NotNull]
        public override string ToString() => $"UnequalNonNullPair {_values}";
    }
}
