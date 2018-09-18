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
using Newtonsoft.Json.Serialization;

namespace Tiger.Hal
{
    /// <summary>Configures a created transformation map.</summary>
    /// <typeparam name="T">The type being transformed.</typeparam>
    [PublicAPI]
    public interface ITransformationMap<T>
    {
        /// <summary>Creates a link for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkData"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link([NotNull] string relation, [NotNull] ILinkData linkData);

        /// <summary>Creates a link for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link([NotNull] string relation, [NotNull] Func<T, ILinkData> linkSelector);

        /// <summary>Creates a link for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="T"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
        /// </param>
        /// <param name="ignore">
        /// If <paramref name="linkSelector"/> is a simple member selector, controls whether the member
        /// selected by <paramref name="linkSelector"/> is not represented in the HAL+JSON serialization
        /// of a value. This value is <see langword="true"/> by default.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <seealso cref="TransformationMapExtensions.Ignore{T, T1}(ITransformationMap{T}, Expression{Func{T, T1}})"/>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link([NotNull] string relation, [NotNull] Expression<Func<T, Uri>> linkSelector, bool ignore = true);

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link<TMember>(
            [NotNull] string relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector);

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection of values of type <typeparamref name="TMember"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link<TMember>(
            [NotNull] string relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<T, TMember, ILinkData> linkSelector);

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="TKey">
        /// The key type of the return type of <paramref name="dictionarySelector"/>.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The value type of the return type of <paramref name="dictionarySelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="dictionarySelector">
        /// A function that selects a dictionary from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="TKey"/> and a value of type <typeparamref name="TValue"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link<TKey, TValue>(
            [NotNull] string relation,
            [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
            [NotNull] Func<TKey, TValue, ILinkData> linkSelector);

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="TKey">
        /// The key type of the return type of <paramref name="dictionarySelector"/>.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The value type of the return type of <paramref name="dictionarySelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="dictionarySelector">
        /// A function that selects a dictionary from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="T"/>, a value of type <typeparamref name="TKey"/>,
        /// and a value of type <typeparamref name="TValue"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        ITransformationMap<T> Link<TKey, TValue>(
            [NotNull] string relation,
            [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
            [NotNull] Func<T, TKey, TValue, ILinkData> linkSelector);

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
        /// Hoists a member that would not be present in the HAL representation of an array value
        /// to the containing object value.
        /// </summary>
        /// <typeparam name="TMember">The type of the value to hoist.</typeparam>
        /// <param name="memberSelector">A function selecting a top-level member to hoist.</param>
        /// <returns>The modified transformation map.</returns>
        /// <remarks>
        /// This only has a meaningful effect on types which resolve to a <see cref="JsonArrayContract"/> --
        /// types which are not serialized as objects. Types which are serialized as objects already have
        /// their member keys at the root level.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        [NotNull]
        ITransformationMap<T> Hoist<TMember>([NotNull] Expression<Func<T, TMember>> memberSelector);

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
