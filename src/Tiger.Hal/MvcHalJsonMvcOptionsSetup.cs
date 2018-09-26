// <copyright file="MvcHalJsonMvcOptionsSetup.cs" company="Cimpress, Inc.">
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
