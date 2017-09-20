using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Tiger.Hal
{
    /// <summary>Extends the functionality of <see cref="IServiceCollection"/> for HAL+JSON.</summary>
    [PublicAPI]
    public static class HalJsonMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Adds HAL+JSON transformation and serialization to the application.
        /// </summary>
        /// <typeparam name="TProfile">The type of the profile to add.</typeparam>
        /// <param name="builder">The application's MVC builder.</param>
        /// <returns>The modified MVC core builder.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">A profile could not be added to the repository builder.</exception>
        [NotNull]
        public static IMvcCoreBuilder AddHalJson<TProfile>([NotNull] this IMvcCoreBuilder builder)
            where TProfile : class, IHalProfile
        {
            if (builder == null) { throw new ArgumentNullException(nameof(builder)); }

            builder.Services.AddSingleton<IHalProfile, TProfile>();
            builder.Services.AddSingleton<HalRepositoryBuilder>();
            builder.Services.AddSingleton(p =>
            {
                var profile = p.GetRequiredService<IHalProfile>();
                var repoBuilder = p.GetRequiredService<HalRepositoryBuilder>();
                return repoBuilder.Build(profile);
            });

            return builder.AddHalJsonFormatter();
        }

        [NotNull]
        static IMvcCoreBuilder AddHalJsonFormatter([NotNull] this IMvcCoreBuilder builder)
        {
            AddHalJsonFormatterServices(builder.Services);
            return builder;
        }

        static void AddHalJsonFormatterServices([NotNull] IServiceCollection services)
        {
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.TryAddEnumerable(Transient<IConfigureOptions<MvcJsonOptions>, ConfigureMvcJsonOptions>());
            services.TryAddEnumerable(Transient<IConfigureOptions<MvcOptions>, MvcHalJsonMvcOptionsSetup>());
        }
    }
}
