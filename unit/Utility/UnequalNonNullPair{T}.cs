using JetBrains.Annotations;
// ReSharper disable All

namespace Tiger.Hal.UnitTest.Utility
{
    public struct UnequalNonNullPair<T>
        where T : class
    {
        public T Left => _values.left;
        public T Right => _values.right;

        readonly (T left, T right) _values;

        public UnequalNonNullPair((T left, T right) values)
        {
            _values = values;
        }

        // todo(cosborn) Can this be better?
        [NotNull]
        public override string ToString() => $"UnequalNonNullPair {_values}";
    }
}
