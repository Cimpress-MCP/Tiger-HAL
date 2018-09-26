// <copyright file="HalJsonMvcBuilderExtensions.cs" company="Cimpress, Inc.">
//   Copyright 2018 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tiger.Hal;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>Extends the functionality of <see cref="IServiceCollection"/> for HAL+JSON.</summary>
    [PublicAPI]
    public static class HalJsonMvcBuilderExtensions
    {
        /// <summary>Adds HAL+JSON transformation and serialization to the application.</summary>
        /// <typeparam name="TProfile">The type of the profile to add.</typeparam>
        /// <param name="builder">The application's MVC builder.</param>
        /// <returns>The modified MVC core builder.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">A profile could not be added to the repository builder.</exception>
        [NotNull]
        public static IMvcBuilder AddHalJson<TProfile>([NotNull] this IMvcBuilder builder)
            where TProfile : class, IHalProfile
        {
            if (builder is null) { throw new ArgumentNullException(nameof(builder)); }

            builder.Services.AddSingleton<IHalProfile, TProfile>();
            builder.Services.AddSingleton<HalRepositoryBuilder>();
            builder.Services.AddScoped<ILinkBuilder<LinkData.Constant>, LinkBuilder.Constant>();
            builder.Services.AddScoped<ILinkBuilder<LinkData.Templated>, LinkBuilder.Templated>();
            builder.Services.AddScoped<ILinkBuilder<LinkData.Routed>, LinkBuilder.Routed>();
            builder.Services.AddSingleton(p =>
            {
                var profile = p.GetRequiredService<IHalProfile>();
                var repoBuilder = p.GetRequiredService<HalRepositoryBuilder>();
                return repoBuilder.Build(profile);
            });

            return builder.AddHalJsonFormatter();
        }

        [NotNull]
        static IMvcBuilder AddHalJsonFormatter([NotNull] this IMvcBuilder builder)
        {
            AddHalJsonFormatterServices(builder.Services);
            return builder;
        }

        static void AddHalJsonFormatterServices([NotNull] IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.TryAddEnumerable(Transient<IConfigureOptions<MvcJsonOptions>, ConfigureMvcJsonOptions>());
            services.TryAddEnumerable(Transient<IConfigureOptions<MvcOptions>, MvcHalJsonMvcOptionsSetup>());
        }
    }
}
