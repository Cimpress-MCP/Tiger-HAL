using JetBrains.Annotations;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Tiger.Hal.UnitTest.Utility
{
    [UsedImplicitly(Members)]
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
