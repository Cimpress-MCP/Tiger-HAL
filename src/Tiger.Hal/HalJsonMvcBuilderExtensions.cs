// <copyright file="HalJsonMvcBuilderExtensions.cs" company="Cimpress, Inc.">
//   Copyright 2020 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tiger.Hal;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>Extends the functionality of <see cref="IServiceCollection"/> for HAL+JSON.</summary>
    public static class HalJsonMvcBuilderExtensions
    {
        /// <summary>Adds HAL+JSON transformation and serialization to the application.</summary>
        /// <typeparam name="TProfile">The type of the profile to add.</typeparam>
        /// <param name="builder">The application's MVC builder.</param>
        /// <returns>The modified MVC core builder.</returns>
        /// <exception cref="InvalidOperationException">A profile could not be added to the repository builder.</exception>
        public static IMvcBuilder AddHalJson<TProfile>(this IMvcBuilder builder)
            where TProfile : class, IHalProfile
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _ = builder.Services
                .AddTransient<IHalProfile, TProfile>()
                .AddSingleton<HalRepositoryBuilder>()
                .AddTransient<ILinkBuilder<LinkData.Constant>, LinkBuilder.Constant>()
                .AddTransient<ILinkBuilder<LinkData.Templated>, LinkBuilder.Templated>()
                .AddTransient<ILinkBuilder<LinkData.Endpointed>, LinkBuilder.Routed>()
                .AddSingleton(p =>
                {
                    var profile = p.GetRequiredService<IHalProfile>();
                    var repoBuilder = p.GetRequiredService<HalRepositoryBuilder>();
                    return repoBuilder.Build(profile);
                });

            return builder.AddHalJsonFormatter();
        }

        static IMvcBuilder AddHalJsonFormatter(this IMvcBuilder builder)
        {
            _ = builder.AddNewtonsoftJson(o => o.SerializerSettings.Converters.Add(new LinkCollection.Converter()));

            AddHalJsonFormatterServices(builder.Services);
            return builder;
        }

        static void AddHalJsonFormatterServices(IServiceCollection services) => services
            .AddOptions()
            .ConfigureOptions<MvcHalJsonMvcOptionsSetup>()
            .AddHttpContextAccessor();
    }
}
