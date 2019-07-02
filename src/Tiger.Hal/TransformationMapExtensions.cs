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
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Tiger.Types;
using static Tiger.Hal.LinkData;
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <summary>
    /// Extends the functionality of the <see cref="ITransformationMap{T}"/> interface
    /// and the <see cref="ITransformationMap{TCollection, TElement}"/> interface.</summary>
    [PublicAPI]
    public static class TransformationMapExtensions
    {
        #region Self

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="self">An <see cref="ILinkData"/> for the "self" relation.</param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="self"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Self<T>(
            [NotNull] this ITransformationMap transformationMap,
            [NotNull] ILinkData self)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (self is null) { throw new ArgumentNullException(nameof(self)); }

            return transformationMap.Self<T>(_ => self);
        }

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="self">An <see cref="ILinkData"/> for the "self" relation.</param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="self"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static IElementTransformationMap<TCollection, TElement> Self<TCollection, TElement>(
            [NotNull] this ITransformationMap transformationMap,
            [NotNull] ILinkData self)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (self is null) { throw new ArgumentNullException(nameof(self)); }

            return transformationMap.Self<TCollection, TElement>(_ => self);
        }

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="self">
        /// A function that creates a <see cref="Uri"/>
        /// from a value of type <typeparamref name="T"/>
        /// for the "self" relation.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="self"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Self<T>(
            [NotNull] this ITransformationMap transformationMap,
            [NotNull] Func<T, Uri> self)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (self is null) { throw new ArgumentNullException(nameof(self)); }

            return transformationMap.Self<T>(t => self(t)?.Pipe(Const));
        }

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="self">
        /// A function that creates a <see cref="Uri"/>
        /// from a value of type <typeparamref name="TCollection"/>
        /// for the "self" relation.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="self"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static IElementTransformationMap<TCollection, TElement> Self<TCollection, TElement>(
            [NotNull] this ITransformationMap transformationMap,
            [NotNull] Func<TCollection, Uri> self)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (self is null) { throw new ArgumentNullException(nameof(self)); }

            return transformationMap.Self<TCollection, TElement>(t => self(t)?.Pipe(Const));
        }

        #endregion

        #region Link

        /// <summary>Manually creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="instruction">A linking instruction.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="instruction"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] ILinkInstruction instruction)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (instruction is null) { throw new ArgumentNullException(nameof(instruction)); }

            return transformationMap.Link(relation.AbsoluteUri, instruction);
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<T, ILinkData> selector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.Link(relation, new LinkInstruction<T>(selector));
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection of values of type <typeparamref name="TMember"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<T, TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (collectionSelector is null) { throw new ArgumentNullException(nameof(collectionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation, new ManyLinkInstruction<T>(t => collectionSelector(t).Select(tm => linkSelector(t, tm))));
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkData"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<T> Link<T>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] string relation,
            [NotNull] ILinkData linkData)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (linkData is null) { throw new ArgumentNullException(nameof(linkData)); }

            return transformationMap.Link(relation, _ => linkData);
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkData"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] string relation,
            [NotNull] ILinkData linkData)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (linkData is null) { throw new ArgumentNullException(nameof(linkData)); }

            return transformationMap.Link(relation, _ => linkData);
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
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">Data representing the link for the provided type.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkData"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] ILinkData linkData)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (linkData is null) { throw new ArgumentNullException(nameof(linkData)); }

            return transformationMap.Link(relation.AbsoluteUri, linkData);
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
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
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
        {
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (collectionSelector is null) { throw new ArgumentNullException(nameof(collectionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation, collectionSelector, (_, tm) => linkSelector(tm));
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<TCollection, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (collectionSelector is null) { throw new ArgumentNullException(nameof(collectionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation, collectionSelector, (_, tm) => linkSelector(tm));
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
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TCollection, ILinkData> selector)
            where TCollection : IReadOnlyCollection<TElement>
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

            return transformationMap.Link(relation.AbsoluteUri, t => selector(t)?.Pipe(Const));
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="TCollection"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TCollection, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.Link(relation.AbsoluteUri, t => selector(t)?.Pipe(Const));
        }

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
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
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection from a value of type <typeparamref name="TCollection"/>.
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
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TCollection, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
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
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
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
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection of values of type <typeparamref name="TMember"/>
        /// from a value of type <typeparamref name="TCollection"/>.
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
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TCollection, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<TCollection, TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (collectionSelector is null) { throw new ArgumentNullException(nameof(collectionSelector)); }

            return transformationMap.Link(relation.AbsoluteUri, collectionSelector, linkSelector);
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<T, Option<TMember>> optionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation, t => optionSelector(t).Map(linkSelector).GetValueOrDefault());
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<TCollection, Option<TMember>> optionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation, t => optionSelector(t).Map(linkSelector).GetValueOrDefault());
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<T, Option<TMember>> optionSelector,
            [NotNull] Func<T, TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation, t => optionSelector(t).Map(tm => linkSelector(t, tm)).GetValueOrDefault());
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] string relation,
            [NotNull] Func<TCollection, Option<TMember>> optionSelector,
            [NotNull] Func<TCollection, TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation, t => optionSelector(t).Map(tm => linkSelector(t, tm)).GetValueOrDefault());
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, Option<TMember>> optionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation.AbsoluteUri, optionSelector, linkSelector);
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TCollection, Option<TMember>> optionSelector,
            [NotNull] Func<TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation.AbsoluteUri, optionSelector, linkSelector);
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<T> Link<T, TMember>(
            [NotNull] this ITransformationMap<T> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<T, Option<TMember>> optionSelector,
            [NotNull] Func<T, TMember, ILinkData> linkSelector)
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation.AbsoluteUri, optionSelector, linkSelector);
        }

        /// <summary>Creates an optional link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">The Some type of the return type of <paramref name="optionSelector"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="optionSelector">
        /// A function that selects an optional value from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="optionSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TCollection, Option<TMember>> optionSelector,
            [NotNull] Func<TCollection, TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (optionSelector is null) { throw new ArgumentNullException(nameof(optionSelector)); }
            if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

            return transformationMap.Link(relation.AbsoluteUri, optionSelector, linkSelector);
        }

        #endregion

        #region Link and Ignore

        /// <summary>
        /// Creates a link for the given type and ignores the member selected to create that link.
        /// </summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="T"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
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

            return transformationMap.Link(relation, t => selector.Compile().Invoke(t)?.Pipe(Const)).Ignore(selector);
        }

        /// <summary>
        /// Creates a link for the given type and ignores the member selected to create that link.
        /// </summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="TCollection"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <seealso cref="Link{T}(ITransformationMap{T}, Uri, Func{T, Uri})"/>
        /// <seealso cref="Ignore{T, T1}(ITransformationMap{T}, Expression{Func{T, T1}})"/>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> LinkAndIgnore<TCollection, TElement>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] string relation,
            [NotNull] Expression<Func<TCollection, Uri>> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.Link(relation, t => selector.Compile().Invoke(t)?.Pipe(Const)).Ignore(selector);
        }

        /// <summary>
        /// Creates a link for the given type and ignores the member selected to create that link.
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

        /// <summary>
        /// Creates a link for the given type and ignores the member selected to create that link.
        /// </summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="TCollection"/>.
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
        public static ITransformationMap<TCollection, TElement> LinkAndIgnore<TCollection, TElement>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Expression<Func<TCollection, Uri>> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return transformationMap.LinkAndIgnore(relation.AbsoluteUri, selector);
        }

        #endregion

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

        /// <summary>Creates an embed for the given type, using only the main object.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">The type of the selected value.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Embed<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Expression<Func<TCollection, TMember>> memberSelector,
            [NotNull] Func<TCollection, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
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
        /// <typeparam name="TMember">The type of the selected property.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the embed.
        /// </param>
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

        /// <summary>Creates an embed for the given type, using the main object and the selected object.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="TMember">The type of the selected property.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the embed.
        /// </param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
        /// <param name="linkSelector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type
        /// <typeparamref name="TCollection"/> and a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Embed<TCollection, TElement, TMember>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Uri relation,
            [NotNull] Expression<Func<TCollection, TMember>> memberSelector,
            [NotNull] Func<TCollection, TMember, ILinkData> linkSelector)
            where TCollection : IReadOnlyCollection<TElement>
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

        /// <summary>Causes a member not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
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
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement, T1>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Expression<Func<TCollection, T1>> memberSelector1)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }

            var name = TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelector1);
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
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="T1">The type of the member selected by <paramref name="memberSelector1"/>.</typeparam>
        /// <typeparam name="T2">The type of the member selected by <paramref name="memberSelector2"/>.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
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
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement, T1, T2>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Expression<Func<TCollection, T1>> memberSelector1,
            [NotNull] Expression<Func<TCollection, T2>> memberSelector2)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }
            if (memberSelector2 is null) { throw new ArgumentNullException(nameof(memberSelector2)); }

            var name1 = TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelector1);
            if (name1 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
            }

            var name2 = TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelector2);
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
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <typeparam name="T1">The type of the member selected by <paramref name="memberSelector1"/>.</typeparam>
        /// <typeparam name="T2">The type of the member selected by <paramref name="memberSelector2"/>.</typeparam>
        /// <typeparam name="T3">The type of the member selected by <paramref name="memberSelector3"/>.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
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
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement, T1, T2, T3>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull] Expression<Func<TCollection, T1>> memberSelector1,
            [NotNull] Expression<Func<TCollection, T2>> memberSelector2,
            [NotNull] Expression<Func<TCollection, T3>> memberSelector3)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }
            if (memberSelector2 is null) { throw new ArgumentNullException(nameof(memberSelector2)); }
            if (memberSelector3 is null) { throw new ArgumentNullException(nameof(memberSelector3)); }

            var name1 = TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelector1);
            if (name1 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
            }

            var name2 = TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelector2);
            if (name2 is null)
            {
                throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2));
            }

            var name3 = TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelector3);
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

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
        /// <param name="memberSelectors">A collection of functions, each selecting a top-level member to ignore.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberSelectors"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelectors"/> is malformed.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement>(
            [NotNull] this ITransformationMap<TCollection, TElement> transformationMap,
            [NotNull, ItemNotNull] params Expression<Func<TCollection, object>>[] memberSelectors)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is null) { throw new ArgumentNullException(nameof(transformationMap)); }
            if (memberSelectors is null) { throw new ArgumentNullException(nameof(memberSelectors)); }

            for (var i = 0; i < memberSelectors.Length; i += 1)
            {
                var name = TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelectors[i]);
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
