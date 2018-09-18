// <copyright file="IHalRepository.cs" company="Cimpress, Inc.">
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

namespace Tiger.Hal
{
    /// <summary>Maps types to type transformers.</summary>
    public interface IHalRepository
    {
        /// <summary>
        /// Determines whether a type has a registered transformer.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="type"/> has a registered transformer,
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool CanTransform([NotNull] Type type);

        /// <summary>
        /// Gets the transformer that is associated with the given type.
        /// </summary>
        /// <param name="type">The type to locate.</param>
        /// <param name="transformer">
        /// When this method returns, the transformer associated with the given type, if the key is found;
        /// otherwise, the default value of <see cref="ITypeTransformer"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if there is a transformer associated with <paramref name="type"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        [ContractAnnotation("=> true, transformer:notnull; => false, transformer:null")]
        bool TryGetTransformer([NotNull] Type type, out ITypeTransformer transformer);
    }
}
