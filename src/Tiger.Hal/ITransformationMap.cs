// <copyright file="ITransformationMap.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Defines a series of transformations for a type to its HAL representation.</summary>
    [PublicAPI]
    public interface ITransformationMap
    {
        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="self">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>
        /// for the "self" relation.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="self"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Self<T>([NotNull] Func<T, ILinkData> self);

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="self">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TCollection"/>
        /// for the "self" relation.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="self"/> is <see langword="null"/>.</exception>
        [NotNull]
        IElementTransformationMap<TCollection, TElement> Self<TCollection, TElement>(
            [NotNull] Func<TCollection, ILinkData> self)
            where TCollection : IReadOnlyCollection<TElement>;
    }
}
