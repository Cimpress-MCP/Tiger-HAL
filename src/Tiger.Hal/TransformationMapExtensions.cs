// <copyright file="TransformationMapExtensions.cs" company="Cimpress, Inc.">
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
using static Tiger.Hal.LinkData;
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <summary>Extends the functionality of the <see cref="ITransformationMap{T}"/> interface.</summary>
    [PublicAPI]
    public static class TransformationMapExtensions
    {
        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="linkData">Data representing the "self" link for the provided type.</param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkData"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Self<T>(
            [NotNull] this ITransformationMap transformationMap,
            [NotNull] ILinkData linkData)
        {
            if (transformationMap == null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (linkData == null) { throw new ArgumentNullException(nameof(linkData)); }

            return transformationMap.Self<T>(_ => linkData);
        }

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Self<T>(
            [NotNull] this ITransformationMap transformationMap,
            [NotNull] Func<T, Uri> selector)
        {
            if (transformationMap == null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.Self<T>(t => Const(selector(t)));
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">Data representing the link for the provided type.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkData"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] ILinkData linkData)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (linkData is null) { throw new ArgumentNullException(nameof(linkData)); }

            return transformationMap.Link(relation.AbsoluteUri, linkData);
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, ILinkData> selector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.Link(relation.AbsoluteUri, selector);
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="T"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, Uri> selector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.Link(relation.AbsoluteUri, selector);
        }

        /// <summary>
        /// Creates a link for the given type and ignores the member selected to create that links.
        /// </summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="T"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
        /// If the function is not a simple property selector, nothing will be ignored.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <seealso cref="Link{T}(ITransformationMap{T}, Uri, Func{T, Uri})"/>
        /// <seealso cref="Ignore{T, T1}(ITransformationMap{T}, Expression{Func{T, T1}})"/>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> LinkAndIgnore<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] string relation,
            [NotNull] Expression<Func<T, Uri>> selector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap
                .Link(relation, selector.Compile())
                .Ignore(selector);
        }

        /// <summary>
        /// Creates a link for the given type and ignores the member selected to create that links.
        /// </summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="T"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
        /// If the function is not a simple property selector, nothing will be ignored.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <seealso cref="Link{T}(ITransformationMap{T}, Uri, Func{T, Uri})"/>
        /// <seealso cref="Ignore{T, T1}(ITransformationMap{T}, Expression{Func{T, T1}})"/>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> LinkAndIgnore<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Expression<Func<T, Uri>> selector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.LinkAndIgnore(relation.AbsoluteUri, selector);
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (collectionSelector is null) { throw new ArgumentNullException(nameof(collectionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation.AbsoluteUri, collectionSelector, linkSelector);
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection of values of type <typeparamref name="TMember"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<T, TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (collectionSelector is null) { throw new ArgumentNullException(nameof(collectionSelector)); }

            return transformationMap.Link(relation.AbsoluteUri, collectionSelector, linkSelector);
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
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
        /// A function that creates an <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="TKey"/> and a value of type <typeparamref name="TValue"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T, TKey, TValue>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
            [NotNull] Func<TKey, TValue, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (dictionarySelector is null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

            return transformationMap.Link(relation.AbsoluteUri, dictionarySelector, linkSelector);
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
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
        /// A function that creates an <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="T"/>, a value of type <typeparamref name="TKey"/>,
        /// and a value of type <typeparamref name="TValue"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T, TKey, TValue>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
            [NotNull] Func<T, TKey, TValue, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (dictionarySelector is null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

            return transformationMap.Link(relation.AbsoluteUri, dictionarySelector, linkSelector);
        }

        /// <summary>Creates an embed for the given type, using only the main object.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the embed.
        /// </param>
        /// <typeparam name="TMember">The type of the selected value.</typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Embed<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Expression<Func<T, TMember>> memberSelector,
            [NotNull] Func<T, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (memberSelector is null) { throw new ArgumentNullException(nameof(memberSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Embed(relation.AbsoluteUri, memberSelector, linkSelector);
        }

        /// <summary>Creates an embed for the given type, using the main object and the selected object.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the embed.
        /// </param>
        /// <typeparam name="TMember">The type of the selected property.</typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="T"/> and a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Embed<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Expression<Func<T, TMember>> memberSelector,
            [NotNull] Func<T, TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (memberSelector is null) { throw new ArgumentNullException(nameof(memberSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Embed(relation.AbsoluteUri, memberSelector, linkSelector);
        }

        /// <summary>Causes a member not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
        /// <typeparam name="T1">The type of the member selected by <paramref name="memberSelector1"/>.</typeparam>
        /// <param name="memberSelector1">
        /// A function selecting a top-level member of type <typeparamref name="T1"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        [NotNull]
        public static ITransformationMap<T> Ignore<T, T1>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Expression<Func<T, T1>> memberSelector1)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }

            var name = TransformationMap.Builder<T>.GetSelectorName(memberSelector1);
            if (name is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
            }

            return transformationMap.Ignore(name);
        }

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
        /// <typeparam name="T1">The type of the member selected by <paramref name="memberSelector1"/>.</typeparam>
        /// <typeparam name="T2">The type of the member selected by <paramref name="memberSelector2"/>.</typeparam>
        /// <param name="memberSelector1">
        /// A function selecting a top-level member of type <typeparamref name="T1"/> to ignore.
        /// </param>
        /// <param name="memberSelector2">
        /// A function selecting a top-level member of type <typeparamref name="T2"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector2"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
        [NotNull]
        public static ITransformationMap<T> Ignore<T, T1, T2>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Expression<Func<T, T1>> memberSelector1,
            [NotNull] Expression<Func<T, T2>> memberSelector2)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }
            if (memberSelector2 is null) { throw new ArgumentNullException(nameof(memberSelector2)); }

            var name1 = TransformationMap.Builder<T>.GetSelectorName(memberSelector1);
            if (name1 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
            }

            var name2 = TransformationMap.Builder<T>.GetSelectorName(memberSelector2);
            if (name2 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2));
            }

            return transformationMap.Ignore(name1).Ignore(name2);
        }

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
        /// <typeparam name="T1">The type of the member selected by <paramref name="memberSelector1"/>.</typeparam>
        /// <typeparam name="T2">The type of the member selected by <paramref name="memberSelector2"/>.</typeparam>
        /// <typeparam name="T3">The type of the member selected by <paramref name="memberSelector3"/>.</typeparam>
        /// <param name="memberSelector1">
        /// A function selecting a top-level member of type <typeparamref name="T1"/> to ignore.
        /// </param>
        /// <param name="memberSelector2">
        /// A function selecting a top-level member of type <typeparamref name="T2"/> to ignore.
        /// </param>
        /// <param name="memberSelector3">
        /// A function selecting a top-level member of type <typeparamref name="T3"/> to ignore.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector2"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector3"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector3"/> is malformed.</exception>
        [NotNull]
        public static ITransformationMap<T> Ignore<T, T1, T2, T3>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Expression<Func<T, T1>> memberSelector1,
            [NotNull] Expression<Func<T, T2>> memberSelector2,
            [NotNull] Expression<Func<T, T3>> memberSelector3)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }
            if (memberSelector2 is null) { throw new ArgumentNullException(nameof(memberSelector2)); }
            if (memberSelector3 is null) { throw new ArgumentNullException(nameof(memberSelector3)); }

            var name1 = TransformationMap.Builder<T>.GetSelectorName(memberSelector1);
            if (name1 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
            }

            var name2 = TransformationMap.Builder<T>.GetSelectorName(memberSelector2);
            if (name2 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2));
            }

            var name3 = TransformationMap.Builder<T>.GetSelectorName(memberSelector3);
            if (name3 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector3));
            }

            return transformationMap.Ignore(name1).Ignore(name2).Ignore(name3);
        }

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
        /// <param name="memberSelectors">A collection of functions, each selecting a top-level member to ignore.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelectors"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelectors"/> is malformed.</exception>
        [NotNull]
        public static ITransformationMap<T> Ignore<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull, ItemNotNull] params Expression<Func<T, object>>[] memberSelectors)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelectors is null) { throw new ArgumentNullException(nameof(memberSelectors)); }

            for (var i = 0; i < memberSelectors.Length; i += 1)
            {
                var name = TransformationMap.Builder<T>.GetSelectorName(memberSelectors[i]);
                if (name is null)
                {
                    throw new ArgumentException(MalformedValueSelector, nameof(memberSelectors))
                    {
                        Data =
                        {
                            ["selector"] = memberSelectors[i]
                        }
                    };
                }

                transformationMap.Ignore(name);
            }

            return transformationMap;
        }
    }
}
