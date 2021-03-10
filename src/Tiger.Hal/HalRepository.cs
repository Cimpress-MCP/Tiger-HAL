// <copyright file="HalRepository.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        public HalRepository(
            IReadOnlyDictionary<Type, ITransformationInstructions> transformations,
            IServiceProvider serviceProvider)
        {
            _transformations = transformations;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        bool IHalRepository.CanTransform(Type type) => _transformations.ContainsKey(type);

        /// <inheritdoc/>
        bool IHalRepository.TryGetTransformer(Type type, [MaybeNullWhen(returnValue: false)] out ITypeTransformer transformer)
        {
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
