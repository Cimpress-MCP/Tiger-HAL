// <copyright file="ITransformationMap{T}.cs" company="Cimpress, Inc.">
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
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Configures a created transformation map.</summary>
    /// <typeparam name="T">The type being transformed.</typeparam>
    [PublicAPI]
    public interface ITransformationMap<T>
    {
        /// <summary>Manually creates a link for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="instruction">A linking instruction.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="instruction"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link([NotNull] string relation, [NotNull] ILinkInstruction instruction);

        /// <summary>Creates an embed for the given type, using only the main object.</summary>
        /// <typeparam name="TMember">The type of the selected value.</typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Embed<TMember>(
            [NotNull] string relation,
            [NotNull] Expression<Func<T, TMember>> memberSelector,
            [NotNull] Func<T, ILinkData> linkSelector);

        /// <summary>Creates an embed for the given type, using the main object and the selected object.</summary>
        /// <typeparam name="TMember">The type of the selected property.</typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="T"/> and a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Embed<TMember>(
            [NotNull] string relation,
            [NotNull] Expression<Func<T, TMember>> memberSelector,
            [NotNull] Func<T, TMember, ILinkData> linkSelector);

        /// <summary>
        /// Creates an embed for the given type, using the main object and the elements of the selected object.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements of the selected value.</typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">A function selecting a top-level member whose elements to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="collectionSelector"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> EmbedElements<TElement>(
            [NotNull] string relation,
            [NotNull] Expression<Func<T, IReadOnlyCollection<TElement>>> collectionSelector,
            [NotNull] Func<TElement, ILinkData> linkSelector);

        /// <summary>Causes a member not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelector1">
        /// The name of a top-level member of type <typeparamref name="T"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Ignore([NotNull] string memberSelector1);

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelector1">
        /// The name of the first top-level member of type <typeparamref name="T"/> to ignore.
        /// </param>
        /// <param name="memberSelector2">
        /// The name of the second top-level member of type <typeparamref name="T"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector2"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Ignore(
            [NotNull] string memberSelector1,
            [NotNull] string memberSelector2);

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelector1">
        /// The name of the first top-level member of type <typeparamref name="T"/> to ignore.
        /// </param>
        /// <param name="memberSelector2">
        /// The name of the second top-level member of type <typeparamref name="T"/> to ignore.
        /// </param>
        /// <param name="memberSelector3">
        /// The name of the third top-level member of type <typeparamref name="T"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector2"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector3"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Ignore(
            [NotNull] string memberSelector1,
            [NotNull] string memberSelector2,
            [NotNull] string memberSelector3);

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelectors">
        /// A collection of top-level members of type <typeparamref name="T"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelectors"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Ignore([NotNull, ItemNotNull] params string[] memberSelectors);
    }
}
