using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Roslynator:Style", "RCS1212", Justification = "Parallel construction.")]
        public override int GetHashCode([CanBeNull] Link obj) => obj is null
            ? 0
            : HashCode.Combine(obj.Href, obj.IsTemplated, obj.Type, obj.Name, obj.Profile, obj.Title, obj.HrefLang);
    }
}
