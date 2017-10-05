namespace Test.Utility
{
    static class UnequalNonNullPair
    {
        public static void Deconstruct<T>(this UnequalNonNullPair<T> pair, out T left, out T right)
            where T : class
        {
            left = pair.Left;
            right = pair.Right;
        }
    }
}
