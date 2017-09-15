using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Test.Utility
{
    /// <summary>Represents a language code, with nation.</summary>
    public class LanguageCode
    {
        readonly string _language;
        readonly string _nation;

        /// <summary>Initializes a new instance of the <see cref="LanguageCode"/> class.</summary>
        /// <param name="language">The "language" portion of the code.</param>
        /// <param name="nation">The "nation" portion of the code.</param>
        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public LanguageCode([NotNull] string language, [NotNull] string nation)
        {
            _language = language?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(language));
            _nation = nation?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(nation));
        }

        /// <inheritdoc/>
        [NotNull, Pure]
        public override string ToString() => $"{_language}-{_nation}";

        /// <summary>Converts a language code to a string.</summary>
        /// <param name="lc">The language code to convert.</param>
        [CanBeNull]
        public static implicit operator string([CanBeNull] LanguageCode lc) => lc?.ToString();
    }
}