using System.Buffers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static JetBrains.Annotations.ImplicitUseKindFlags;

namespace Tiger.Hal
{
    /* todo(cosborn)
     * I find this style a little awkward, and I'd rather implement
     * IConfigureOptions<MvcOptions> than inherit. But I've cribbed
     * from the configuration for the usual JSON formatter, and I'm
     * unwilling to break from it quite yet. Once I know that there
     * will be no weird lifetime issues created by it, I'll change.
     */

    /// <summary>Configures the options object that represents MVC configuration.</summary>
    [UsedImplicitly(InstantiatedNoFixedConstructorSignature)]
    sealed class MvcHalJsonMvcOptionsSetup
        : ConfigureOptions<MvcOptions>
    {
        /// <summary>Initializes a new instance of the <see cref="MvcHalJsonMvcOptionsSetup"/> class.</summary>
        /// <param name="jsonOptions">The application's MVC JSON configuration.</param>
        /// <param name="halRepository">The application's HAL+JSON repository.</param>
        /// <param name="charPool">A pool of <see cref="char"/>.</param>
        public MvcHalJsonMvcOptionsSetup(
            IOptions<MvcJsonOptions> jsonOptions,
            IHalRepository halRepository,
            ArrayPool<char> charPool)
            : base(options => ConfigureMvc(
                options,
                halRepository,
                jsonOptions.Value.SerializerSettings,
                charPool))
        {
        }

        /// <summary>Configures the options.</summary>
        /// <param name="options">The application's MVC JSON configuration.</param>
        /// <param name="halRepository">The application's HAL+JSON repository.</param>
        /// <param name="serializerSettings">The application's JSON serialization settings.</param>
        /// <param name="charPool">A pool of <see cref="char"/>.</param>
        static void ConfigureMvc(
            [NotNull] MvcOptions options,
            [NotNull] IHalRepository halRepository,
            [NotNull] JsonSerializerSettings serializerSettings,
            [NotNull] ArrayPool<char> charPool)
        {
            var outputFormatter = new HalJsonOutputFormatter(halRepository, serializerSettings, charPool);
            options.OutputFormatters.Add(outputFormatter);
        }
    }
}
