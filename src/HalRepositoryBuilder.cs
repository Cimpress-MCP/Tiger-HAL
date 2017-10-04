// <copyright file="HalRepositoryBuilder.cs" company="Cimpress, Inc.">
//   Copyright 2017 Cimpress, Inc.
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
using System.Collections.Immutable;
using JetBrains.Annotations;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Tiger.Hal
{
    /// <summary>Builds an <see cref="IHalRepository"/> from <see cref="IHalProfile"/>s.</summary>
    [UsedImplicitly(Members)]
    sealed class HalRepositoryBuilder
    {
        readonly IServiceProvider _serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="HalRepositoryBuilder"/> class.</summary>
        /// <param name="serviceProvider">The application's service provider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
        public HalRepositoryBuilder([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>Builds an <see cref="IHalRepository"/>.</summary>
        /// <param name="profile">
        /// The profile containing the declarations for creating a transformation mapping.
        /// </param>
        /// <returns>The built <see cref="IHalRepository"/>.</returns>
        [NotNull]
        public IHalRepository Build([NotNull] IHalProfile profile)
        {
            var transformationMap = new TransformationMap();
            profile.OnTransformationMapCreating(transformationMap);
            var transformations = transformationMap.Maps.ToImmutableDictionary();
            return new HalRepository(transformations, _serviceProvider);
        }
    }
}
