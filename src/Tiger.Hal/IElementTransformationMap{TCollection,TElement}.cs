// <copyright file="IElementTransformationMap{TCollection,TElement}.cs" company="Cimpress, Inc.">
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
    /// <summary>Configures a created transformation map.</summary>
    /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
    /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
    public interface IElementTransformationMap<TCollection, TElement>
        where TCollection : IReadOnlyCollection<TElement>
    {
        /// <summary>Creates links to the elements for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<TCollection, TElement> LinkElements(
            [NotNull] string relation,
            [NotNull] Func<TElement, ILinkData> selector);

        /// <summary>Creates embeds of the elements for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<TCollection, TElement> EmbedElements(
            [NotNull] string relation,
            [NotNull] Func<TElement, ILinkData> selector);
    }
}
