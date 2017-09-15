using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static JetBrains.Annotations.ImplicitUseKindFlags;

namespace Tiger.Hal
{
    /// <summary>Configures the options object that represents JSON formatting configuration.</summary>
    [UsedImplicitly(InstantiatedNoFixedConstructorSignature)]
    sealed class ConfigureMvcJsonOptions
        : IConfigureOptions<MvcJsonOptions>
    {
        /// <inheritdoc/>
        void IConfigureOptions<MvcJsonOptions>.Configure([NotNull] MvcJsonOptions options)
        {
            options.SerializerSettings.Converters.Add(new LinkCollection.Converter());
        }
    }
}
