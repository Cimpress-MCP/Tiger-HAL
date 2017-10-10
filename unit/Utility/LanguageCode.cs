using System;
using JetBrains.Annotations;

namespace Test.Utility
{
    /// <summary>Represents a language code, with nation.</summary>
    public struct LanguageCode
    {
        readonly string _languageCode;

        /// <summary>Initializes a new instance of the <see cref="LanguageCode"/> struct.</summary>
        /// <param name="languageCode">The raw string language code.</param>
        public LanguageCode([NotNull] string languageCode)
        {
            _languageCode = languageCode ?? throw new ArgumentNullException(nameof(languageCode));
        }

        /// <inheritdoc/>
        [NotNull, Pure]
        public override string ToString() => _languageCode;

        /// <summary>Converts a language code to a string.</summary>
        /// <param name="lc">The language code to convert.</param>
        [NotNull]
        public static implicit operator string(LanguageCode lc) => lc.ToString();
    }
}
