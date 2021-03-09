// <copyright file="HalRepositoryBuilder.cs" company="Cimpress, Inc.">
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

namespace Tiger.Hal
{
    /// <summary>Builds an <see cref="IHalRepository"/> from <see cref="IHalProfile"/>s.</summary>
    sealed class HalRepositoryBuilder
    {
        readonly IServiceProvider _serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="HalRepositoryBuilder"/> class.</summary>
        /// <param name="serviceProvider">The application's service provider.</param>
        public HalRepositoryBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>Builds an <see cref="IHalRepository"/>.</summary>
        /// <param name="profile">
        /// The profile containing the declarations for creating a transformation mapping.
        /// </param>
        /// <returns>The built <see cref="IHalRepository"/>.</returns>
        public IHalRepository Build(IHalProfile profile)
        {
            var transformationMap = new TransformationMap();
            profile.OnTransformationMapCreating(transformationMap);
            return new HalRepository(transformationMap.Maps, _serviceProvider);
        }
    }
}
