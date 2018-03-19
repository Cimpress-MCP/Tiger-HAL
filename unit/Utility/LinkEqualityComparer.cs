using System.Collections.Generic;
using JetBrains.Annotations;
using Tiger.Hal;
using static System.StringComparison;

namespace Test.Utility
{
    /// <summary>Compares two instances of <see cref="Link"/> for equality.</summary>
    sealed class LinkEqualityComparer
        : EqualityComparer<Link>
    {
        /// <inheritdoc/>
        public override bool Equals([CanBeNull] Link x, [CanBeNull] Link y)
        {
            if (ReferenceEquals(x, y)) { return true; }
            if (x is null || y is null) { return false; }

            return string.Equals(x.Href, y.Href, Ordinal)
                && x.IsTemplated == y.IsTemplated
                && string.Equals(x.Type, y.Type, Ordinal)
                && x.Deprecation == y.Deprecation
                && string.Equals(x.Name, y.Name, Ordinal)
                && x.Profile == y.Profile
                && string.Equals(x.Title, y.Title, Ordinal)
                && string.Equals(x.HrefLang, y.HrefLang, OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override int GetHashCode([CanBeNull] Link obj)
        {
            if (obj is null) { return 0; }

            unchecked
            {
                var hash = 17;
                hash = hash * 23 + obj.Href.GetHashCode();
                hash = hash * 23 + obj.IsTemplated.GetHashCode();
                hash = hash * 23 + obj.Type?.GetHashCode() ?? 0;
                hash = hash * 23 + obj.Name?.GetHashCode() ?? 0;
                hash = hash * 23 + obj.Profile?.GetHashCode() ?? 0;
                hash = hash * 23 + obj.Title?.GetHashCode() ?? 0;
                hash = hash * 23 + obj.HrefLang?.GetHashCode() ?? 0;
                return hash;
            }
        }
    }
}
