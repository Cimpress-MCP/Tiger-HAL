using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
        // note(cosborn) Cached.
        static readonly TypeInfo _halProfile = typeof(HalProfile).GetTypeInfo();

        /// <summary>
        /// Adds HAL+JSON transformation and serialization to the application.
        /// </summary>
        /// <param name="builder">The application's MVC builder.</param>
        /// <param name="assemblyMarkerTypes">
        /// A collection of types whose assemblies will be inspected for
        /// implementations of <see cref="HalProfile"/>.
        /// </param>
        /// <returns>The modified MVC core builder.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">A profile could not be added to the repository builder.</exception>
        [NotNull]
        [SuppressMessage(
            "ReSharper",
            "ExceptionNotDocumented",
            Justification = "The remaining uncaught exception types cannot occur by this method of loading them.")]
        public static IMvcCoreBuilder AddHalJson(
            [NotNull] this IMvcCoreBuilder builder,
            [NotNull] params Type[] assemblyMarkerTypes)
        {
            if (builder == null) { throw new ArgumentNullException(nameof(builder)); }

            var profileTypes = assemblyMarkerTypes
                .Select(t => t.GetTypeInfo())
                .Select(i => i.Assembly)
                .SelectMany(a => a.DefinedTypes)
                .Where(_halProfile.IsAssignableFrom)
                .Where(i => !i.IsAbstract)
                .Select(i => i.AsType());

            var configurator = new HalRepositoryBuilder();
            foreach (var profileType in profileTypes)
            {
                try
                {
                    configurator.AddProfile(profileType);
                }
                catch (MissingMethodException mme)
                {
                    throw new InvalidOperationException(
                        $@"The type ""{profileType.FullName}"" does not have a parameterless constructor",
                        mme);
                }
                catch (TypeLoadException tle)
                {
                    throw new InvalidOperationException(
                        $@"The type ""{profileType.FullName}"" is not a valid type.",
                        tle);
                }
                catch (MethodAccessException mae)
                { // note(cosborn) If anyone trips this one, I will be surprised.
                    throw new InvalidOperationException(
                        $@"The process lacks permission to construct an instance of {profileType.FullName}.",
                        mae);
                }
                catch (TargetInvocationException tie)
                {
                    throw new InvalidOperationException(
                        $@"The default constructor of {profileType.FullName} threw an exception on invocation.",
                        tie);
                }

                // note(cosborn) And just panic, otherwise.
            }

            builder.Services.AddSingleton(p =>
            {
                var actionContextAccessor = p.GetRequiredService<IActionContextAccessor>();
                var urlHelperFactory = p.GetRequiredService<IUrlHelperFactory>();
                return configurator.Build(actionContextAccessor, urlHelperFactory);
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
            services.TryAddEnumerable(Transient<IConfigureOptions<MvcOptions>, MvcHalJsonMvcOptionsSetup>());
        }
    }
}
