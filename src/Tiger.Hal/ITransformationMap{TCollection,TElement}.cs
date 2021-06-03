// <copyright file="ITransformationMap{TCollection,TElement}.cs" company="Cimpress, Inc.">
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
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;

namespace Tiger.Hal
{
    /// <summary>Configures a created transformation map.</summary>
    /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
    /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
    public interface ITransformationMap<TCollection, TElement>
        where TCollection : IReadOnlyCollection<TElement>
    {
        /// <summary>
        /// Hoists a member that would not be present in the HAL representation of an array value
        /// to the containing object value.
        /// </summary>
        /// <typeparam name="TMember">The type of the value to hoist.</typeparam>
        /// <param name="selector">A function selecting a top-level member to hoist.</param>
        /// <returns>The modified transformation map.</returns>
        /// <remarks>
        /// This only has a meaningful effect on types which resolve to a <see cref="JsonArrayContract"/> --
        /// types which are not serialized as objects. Types which are serialized as objects already have
        /// their member keys at the root level.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="selector"/> is malformed.</exception>
        ITransformationMap<TCollection, TElement> Hoist<TMember>(Expression<Func<TCollection, TMember>> selector);

        /// <summary>Creates a link for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        ITransformationMap<TCollection, TElement> Link(string relation, Func<TCollection, ILinkData?> selector);

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection of values of type <typeparamref name="TMember"/>
        /// from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        ITransformationMap<TCollection, TElement> Link<TMember>(
            string relation,
            Func<TCollection, IEnumerable<TMember>> collectionSelector,
            Func<TCollection, TMember, ILinkData?> linkSelector);

        /// <summary>Creates an embed for the given type, using only the main object.</summary>
        /// <typeparam name="TMember">The type of the selected value.</typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        ITransformationMap<TCollection, TElement> Embed<TMember>(
            string relation,
            Expression<Func<TCollection, TMember>> memberSelector,
            Func<TCollection, ILinkData?> linkSelector);

        /// <summary>Creates an embed for the given type, using the main object and the selected object.</summary>
        /// <typeparam name="TMember">The type of the selected property.</typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="TCollection"/> and a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        ITransformationMap<TCollection, TElement> Embed<TMember>(
            string relation,
            Expression<Func<TCollection, TMember>> memberSelector,
            Func<TCollection, TMember, ILinkData?> linkSelector);

        /// <summary>Causes a member not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelector1">
        /// The name of a top-level member of type <typeparamref name="TCollection"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        ITransformationMap<TCollection, TElement> Ignore(string memberSelector1);

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelector1">
        /// The name of the first top-level member of type <typeparamref name="TCollection"/> to ignore.
        /// </param>
        /// <param name="memberSelector2">
        /// The name of the second top-level member of type <typeparamref name="TCollection"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        ITransformationMap<TCollection, TElement> Ignore(
            string memberSelector1,
            string memberSelector2);

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelector1">
        /// The name of the first top-level member of type <typeparamref name="TCollection"/> to ignore.
        /// </param>
        /// <param name="memberSelector2">
        /// The name of the second top-level member of type <typeparamref name="TCollection"/> to ignore.
        /// </param>
        /// <param name="memberSelector3">
        /// The name of the third top-level member of type <typeparamref name="TCollection"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        ITransformationMap<TCollection, TElement> Ignore(
            string memberSelector1,
            string memberSelector2,
            string memberSelector3);

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <param name="memberSelectors">
        /// A collection of top-level members of type <typeparamref name="TCollection"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        ITransformationMap<TCollection, TElement> Ignore(params string[] memberSelectors);
    }
}
