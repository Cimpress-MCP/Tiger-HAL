// <copyright file="HalRepository.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <inheritdoc/>
    sealed class HalRepository
        : IHalRepository
    {
        readonly IReadOnlyDictionary<Type, ITransformationInstructions> _transformations;
        readonly IServiceProvider _serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="HalRepository"/> class.</summary>
        /// <param name="transformations">A mapping of types to type transformation maps.</param>
        /// <param name="serviceProvider">The application's service provider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transformations"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
        public HalRepository(
            [NotNull] IReadOnlyDictionary<Type, ITransformationInstructions> transformations,
            [NotNull] IServiceProvider serviceProvider)
        {
            _transformations = transformations ?? throw new ArgumentNullException(nameof(transformations));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        bool IHalRepository.CanTransform(Type type)
        {
            if (type is null) { throw new ArgumentNullException(nameof(type)); }

            return _transformations.ContainsKey(type);
        }

        /// <inheritdoc/>
        bool IHalRepository.TryGetTransformer(Type type, out ITypeTransformer transformer)
        {
            if (type is null) { throw new ArgumentNullException(nameof(type)); }

            if (!_transformations.TryGetValue(type, out var transformationMap))
            {
                transformer = default;
                return false;
            }

            transformer = new TypeTransformer(transformationMap, _serviceProvider);
            return true;
        }
    }
}
