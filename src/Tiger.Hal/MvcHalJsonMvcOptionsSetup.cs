// <copyright file="MvcHalJsonMvcOptionsSetup.cs" company="Cimpress, Inc.">
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

using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Tiger.Hal
{
    /// <summary>Configures the options object that represents MVC configuration.</summary>
    sealed class MvcHalJsonMvcOptionsSetup
        : IConfigureOptions<MvcOptions>
    {
        readonly MvcNewtonsoftJsonOptions _jsonOptions;
        readonly ArrayPool<char> _charPool;
        readonly IHalRepository _halRepository;

        /// <summary>Initializes a new instance of the <see cref="MvcHalJsonMvcOptionsSetup"/> class.</summary>
        /// <param name="jsonOptions">The application's MVC JSON configuration.</param>
        /// <param name="charPool">A pool of <see cref="char"/>.</param>
        /// <param name="halRepository">The application's HAL+JSON repository.</param>
        public MvcHalJsonMvcOptionsSetup(
            IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
            ArrayPool<char> charPool,
            IHalRepository halRepository)
        {
            _jsonOptions = jsonOptions.Value;
            _charPool = charPool;
            _halRepository = halRepository;
        }

        /// <inheritdoc/>
        void IConfigureOptions<MvcOptions>.Configure(MvcOptions options)
        {
            var outputFormatter = new HalJsonOutputFormatter(_jsonOptions.SerializerSettings, _charPool, options, _halRepository);
            options.OutputFormatters.Add(outputFormatter);
        }
    }
}
