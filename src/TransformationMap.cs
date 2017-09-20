// <copyright file="TransformationMap.cs" company="Cimpress, Inc.">
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
    /// <summary>Defines a series of transformations for a type to its HAL representation.</summary>
    [PublicAPI]
    public sealed partial class TransformationMap
    {
        /// <summary>Gets the mapping of transformation instructions.</summary>
        internal IDictionary<Type, ITransformationInstructions> Maps { get; } =
            new Dictionary<Type, ITransformationInstructions>();

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="selector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public ITransformationMap<T> Self<T>([NotNull] Func<T, LinkBuilder> selector)
        {
            if (selector == null) { throw new ArgumentNullException(nameof(selector)); }

            var builder = new Builder<T>(selector);
            Maps[typeof(T)] = builder;
            return builder;
        }
    }
}
