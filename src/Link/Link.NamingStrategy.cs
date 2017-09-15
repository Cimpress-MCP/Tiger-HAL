using Newtonsoft.Json.Serialization;

namespace Tiger.Hal
{
    /// <content>JSON serialization naming strategy.</content>
    public sealed partial class Link
    {
        /// <summary>
        /// Defines the strategy for naming keys in the JSON serialization of <see cref="Link"/>.
        /// </summary>
        sealed class NamingStrategy
            : CamelCaseNamingStrategy
        {
            /// <inheritdoc/>
            public override string GetPropertyName(string name, bool hasSpecifiedName)
            {
                switch (name)
                {
                    case nameof(IsTemplated):
                        return "templated";
                    case nameof(HrefLang):
                        return "hreflang";
                    default:
                        return base.GetPropertyName(name, hasSpecifiedName);
                }
            }
        }
    }
}
