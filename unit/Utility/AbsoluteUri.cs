using System;
using JetBrains.Annotations;

namespace Test.Utility
{
    /// <summary>Represents a <see cref="Uri"/> with, at least, protocol and domain.</summary>
    struct AbsoluteUri
    {
        readonly Uri _uri;

        /// <summary>Initializes a new instance of the <see cref="AbsoluteUri"/> struct.</summary>
        /// <param name="uri">The <see cref="Uri"/> from which to build this value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not absolute.</exception>
        public AbsoluteUri([NotNull] Uri uri)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }
            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException("URI must be absolute.", nameof(uri));
            }
            _uri = uri;
        }

        /// <summary>Converts an absolute URI to an undistinguished URI.</summary>
        /// <returns>The underlying instance of <see cref="Uri"/>.</returns>
        [NotNull, Pure]
        public Uri ToUri() => _uri;

        /// <inheritdoc/>
        [NotNull, Pure]
        public override string ToString() => _uri.ToString();

        /// <summary>Converts an absolute URI to an undistinguished URI.</summary>
        /// <param name="uri">The URI to convert.</param>
        [NotNull]
        public static implicit operator Uri(AbsoluteUri uri) => uri._uri;
    }
}
