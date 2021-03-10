// <copyright file="TransformationMapExtensions.cs" company="Cimpress, Inc.">
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
using System.Linq;
using System.Linq.Expressions;
using Tiger.Types;
using static Tiger.Hal.LinkData;
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <summary>
    /// Extends the functionality of the <see cref="ITransformationMap{T}"/> interface
    /// and the <see cref="ITransformationMap{TCollection, TElement}"/> interface.</summary>
    public static class TransformationMapExtensions
    {
        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="self">An <see cref="ILinkData"/> for the "self" relation.</param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        public static ITransformationMap<T> Self<T>(
            this ITransformationMap transformationMap,
            ILinkData self) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Self<T>(_ => self);

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="self">An <see cref="ILinkData"/> for the "self" relation.</param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        public static IElementTransformationMap<TCollection, TElement> Self<TCollection, TElement>(
            this ITransformationMap transformationMap,
            ILinkData self)
            where TCollection : IReadOnlyCollection<TElement> => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Self<TCollection, TElement>(_ => self);

        /// <summary>Creates the "self" link relation for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="self">
        /// A function that creates a <see cref="Uri"/>
        /// from a value of type <typeparamref name="T"/>
        /// for the "self" relation.
        /// </param>
        /// <returns>A transformation map from which further transformations can be defined.</returns>
        public static ITransformationMap<T> Self<T>(
            this ITransformationMap transformationMap,
            Func<T, Uri> self) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Self<T>(t => self(t).Pipe(Const));

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
        public static IElementTransformationMap<TCollection, TElement> Self<TCollection, TElement>(
            this ITransformationMap transformationMap,
            Func<TCollection, Uri> self)
            where TCollection : IReadOnlyCollection<TElement> => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Self<TCollection, TElement>(t => self(t).Pipe(Const));

        /// <summary>Manually creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="instruction">A linking instruction.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            ILinkInstruction instruction) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, instruction),
            };

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        public static ITransformationMap<T> Link<T>(
            this ITransformationMap<T> transformationMap,
            string relation,
            Func<T, ILinkData?> selector) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, new LinkInstruction<T>(selector));

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
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            string relation,
            Func<T, IEnumerable<TMember>> collectionSelector,
            Func<T, TMember, ILinkData?> linkSelector) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, new ManyLinkInstruction<T>(t => collectionSelector(t).Select(tm => linkSelector(t, tm))));

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        public static ITransformationMap<T> Link<T>(
            this ITransformationMap<T> transformationMap,
            string relation,
            ILinkData linkData) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, _ => linkData);

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
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            string relation,
            ILinkData linkData)
            where TCollection : IReadOnlyCollection<TElement> => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, _ => linkData);

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">Data representing the link for the provided type.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            ILinkData linkData) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, linkData),
            };

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkData">Data representing the link for the provided type.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            ILinkData linkData)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, linkData),
            };

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
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            string relation,
            Func<T, IEnumerable<TMember>> collectionSelector,
            Func<TMember, ILinkData?> linkSelector) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, collectionSelector, (_, tm) => linkSelector(tm));

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TElement">
        /// The element type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        public static ITransformationMap<T> Link<T, TElement>(
            this ITransformationMap<T> transformationMap,
            string relation,
            Func<T, IEnumerable<TElement>> collectionSelector)
            where TElement : ILinkData => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, collectionSelector, (_, te) => te);

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
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            string relation,
            Func<TCollection, IEnumerable<TMember>> collectionSelector,
            Func<TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, collectionSelector, (_, tm) => linkSelector(tm));

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Func<T, ILinkData?> selector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, selector),
            };

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="TCollection"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Func<TCollection, ILinkData?> selector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, selector),
            };

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">The transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/> from a value of type <typeparamref name="T"/>.
        /// If the <see cref="Uri"/> that is created is <see langword="null"/>, no link will be created.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Func<T, Uri> selector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, t => selector(t)?.Pipe(Const)),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Func<TCollection, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, t => selector(t)?.Pipe(Const)),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Func<T, IEnumerable<TMember>> collectionSelector,
            Func<TMember, ILinkData?> linkSelector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, collectionSelector, linkSelector),
            };

        /// <summary>Creates a collection of links for the given type.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <typeparam name="TElement">
        /// The element type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the link.
        /// </param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T, TElement>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Func<T, IEnumerable<TElement>> collectionSelector)
            where TElement : ILinkData => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, collectionSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Func<TCollection, IEnumerable<TMember>> collectionSelector,
            Func<TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, collectionSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Func<T, IEnumerable<TMember>> collectionSelector,
            Func<T, TMember, ILinkData?> linkSelector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, collectionSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Func<TCollection, IEnumerable<TMember>> collectionSelector,
            Func<TCollection, TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, collectionSelector, linkSelector),
            };

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
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            string relation,
            Func<T, Option<TMember>> optionSelector,
            Func<TMember, ILinkData?> linkSelector) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, t => optionSelector(t).Map(linkSelector).GetValueOrDefault());

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
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            string relation,
            Func<TCollection, Option<TMember>> optionSelector,
            Func<TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, t => optionSelector(t).Map(linkSelector).GetValueOrDefault());

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
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            string relation,
            Func<T, Option<TMember>> optionSelector,
            Func<T, TMember, ILinkData?> linkSelector) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, t => optionSelector(t).Map(tm => linkSelector(t, tm)).GetValueOrDefault());

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
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            string relation,
            Func<TCollection, Option<TMember>> optionSelector,
            Func<TCollection, TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, t => optionSelector(t).Map(tm => linkSelector(t, tm)).GetValueOrDefault());

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Func<T, Option<TMember>> optionSelector,
            Func<TMember, ILinkData?> linkSelector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, optionSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Func<TCollection, Option<TMember>> optionSelector,
            Func<TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, optionSelector, linkSelector),
            };

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
        public static ITransformationMap<T> Link<T, TMember>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Func<T, Option<TMember>> optionSelector,
            Func<T, TMember, ILinkData?> linkSelector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, optionSelector, linkSelector),
            };

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
        public static ITransformationMap<TCollection, TElement> Link<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Func<TCollection, Option<TMember>> optionSelector,
            Func<TCollection, TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Link(u, optionSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> LinkAndIgnore<T>(
            this ITransformationMap<T> transformationMap,
            string relation,
            Expression<Func<T, Uri>> selector) => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, t => selector.Compile().Invoke(t)?.Pipe(Const)).Ignore(selector);

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> LinkAndIgnore<TCollection, TElement>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            string relation,
            Expression<Func<TCollection, Uri>> selector)
            where TCollection : IReadOnlyCollection<TElement> => transformationMap is not { } tm
                ? throw new ArgumentNullException(nameof(transformationMap))
                : tm.Link(relation, t => selector.Compile().Invoke(t).Pipe(Const)).Ignore(selector);

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<T> LinkAndIgnore<T>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Expression<Func<T, Uri>> selector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.LinkAndIgnore(u, selector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> LinkAndIgnore<TCollection, TElement>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Expression<Func<TCollection, Uri>> selector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.LinkAndIgnore(u, selector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        public static ITransformationMap<T> Embed<T, TMember>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Expression<Func<T, TMember>> memberSelector,
            Func<T, ILinkData?> linkSelector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Embed(u, memberSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        public static ITransformationMap<TCollection, TElement> Embed<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Expression<Func<TCollection, TMember>> memberSelector,
            Func<TCollection, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Embed(u, memberSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        public static ITransformationMap<T> Embed<T, TMember>(
            this ITransformationMap<T> transformationMap,
            Uri relation,
            Expression<Func<T, TMember>> memberSelector,
            Func<T, TMember, ILinkData?> linkSelector) => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Embed(u, memberSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
        public static ITransformationMap<TCollection, TElement> Embed<TCollection, TElement, TMember>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Uri relation,
            Expression<Func<TCollection, TMember>> memberSelector,
            Func<TCollection, TMember, ILinkData?> linkSelector)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } tm, { AbsoluteUri: { } u }) => tm.Embed(u, memberSelector, linkSelector),
            };

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
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        public static ITransformationMap<T> Ignore<T, T1>(
            this ITransformationMap<T> transformationMap,
            Expression<Func<T, T1>> memberSelector1) => (transformationMap, memberSelector1) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(memberSelector1)),
                ({ } tm, { } ms1) => TransformationMap.Builder<T>.GetSelectorName(ms1) switch
                {
                    { } n => tm.Ignore(n),
                    _ => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1)),
                },
            };

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
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement, T1>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Expression<Func<TCollection, T1>> memberSelector1)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, memberSelector1) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(memberSelector1)),
                ({ } tm, { } ms1) => TransformationMap.Builder<TCollection, TElement>.GetSelectorName(ms1) switch
                {
                    { } n1 => tm.Ignore(n1),
                    _ => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1)),
                },
            };

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
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
        public static ITransformationMap<T> Ignore<T, T1, T2>(
            this ITransformationMap<T> transformationMap,
            Expression<Func<T, T1>> memberSelector1,
            Expression<Func<T, T2>> memberSelector2) => (transformationMap, memberSelector1, memberSelector2) switch
            {
                (null, _, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null, _) => throw new ArgumentNullException(nameof(memberSelector1)),
                (_, _, null) => throw new ArgumentNullException(nameof(memberSelector1)),
                ({ } tm, { } ms1, { } ms2) =>
                    (TransformationMap.Builder<T>.GetSelectorName(ms1), TransformationMap.Builder<T>.GetSelectorName(ms2)) switch
                    {
                        (null, _) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1)),
                        (_, null) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2)),
                        ({ } n1, { } n2) => tm.Ignore(n1, n2),
                    },
            };

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
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement, T1, T2>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Expression<Func<TCollection, T1>> memberSelector1,
            Expression<Func<TCollection, T2>> memberSelector2)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, memberSelector1, memberSelector2) switch
            {
                (null, _, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null, _) => throw new ArgumentNullException(nameof(memberSelector1)),
                (_, _, null) => throw new ArgumentNullException(nameof(memberSelector2)),
                ({ } tm, { } ms1, { } ms2) =>
                    (TransformationMap.Builder<TCollection, TElement>.GetSelectorName(ms1),
                    TransformationMap.Builder<TCollection, TElement>.GetSelectorName(ms2)) switch
                    {
                        (null, _) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1)),
                        (_, null) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2)),
                        ({ } n1, { } n2) => tm.Ignore(n1, n2),
                    },
            };

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
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector3"/> is malformed.</exception>
        public static ITransformationMap<T> Ignore<T, T1, T2, T3>(
            this ITransformationMap<T> transformationMap,
            Expression<Func<T, T1>> memberSelector1,
            Expression<Func<T, T2>> memberSelector2,
            Expression<Func<T, T3>> memberSelector3) => (transformationMap, memberSelector1, memberSelector2, memberSelector3) switch
            {
                (null, _, _, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null, _, _) => throw new ArgumentNullException(nameof(memberSelector1)),
                (_, _, null, _) => throw new ArgumentNullException(nameof(memberSelector2)),
                (_, _, _, null) => throw new ArgumentNullException(nameof(memberSelector3)),
                ({ } tm, { } ms1, { } ms2, { } ms3) =>
                    (TransformationMap.Builder<T>.GetSelectorName(ms1),
                    TransformationMap.Builder<T>.GetSelectorName(ms2),
                    TransformationMap.Builder<T>.GetSelectorName(ms3)) switch
                    {
                        (null, _, _) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1)),
                        (_, null, _) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2)),
                        (_, _, null) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector3)),
                        ({ } n1, { } n2, { } n3) => tm.Ignore(n1, n2, n3),
                    },
            };

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
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelector3"/> is malformed.</exception>
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement, T1, T2, T3>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            Expression<Func<TCollection, T1>> memberSelector1,
            Expression<Func<TCollection, T2>> memberSelector2,
            Expression<Func<TCollection, T3>> memberSelector3)
            where TCollection : IReadOnlyCollection<TElement> => (transformationMap, memberSelector1, memberSelector2, memberSelector3) switch
            {
                (null, _, _, _) => throw new ArgumentNullException(nameof(transformationMap)),
                (_, null, _, _) => throw new ArgumentNullException(nameof(memberSelector1)),
                (_, _, null, _) => throw new ArgumentNullException(nameof(memberSelector2)),
                (_, _, _, null) => throw new ArgumentNullException(nameof(memberSelector3)),
                ({ } tm, { } ms1, { } ms2, { } ms3) =>
                    (TransformationMap.Builder<TCollection, TElement>.GetSelectorName(ms1),
                    TransformationMap.Builder<TCollection, TElement>.GetSelectorName(ms2),
                    TransformationMap.Builder<TCollection, TElement>.GetSelectorName(ms3)) switch
                    {
                        (null, _, _) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1)),
                        (_, null, _) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2)),
                        (_, _, null) => throw new ArgumentException(MalformedValueSelector, nameof(memberSelector3)),
                        ({ } n1, { } n2, { } n3) => tm.Ignore(n1, n2, n3),
                    },
            };

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
        /// <param name="memberSelectors">A collection of functions, each selecting a top-level member to ignore.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelectors"/> is malformed.</exception>
        public static ITransformationMap<T> Ignore<T>(
            this ITransformationMap<T> transformationMap,
            params Expression<Func<T, object>>[] memberSelectors)
        {
            if (transformationMap is not { } tm)
            {
                throw new ArgumentNullException(nameof(transformationMap));
            }

            for (var i = 0; i < memberSelectors.Length; i++)
            {
                if (TransformationMap.Builder<T>.GetSelectorName(memberSelectors[i]) is not { } name)
                {
                    throw new ArgumentException(MalformedValueSelector, nameof(memberSelectors))
                    {
                        Data =
                        {
                            ["selector"] = memberSelectors[i],
                        },
                    };
                }

                _ = tm.Ignore(name);
            }

            return tm;
        }

        /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="transformationMap">
        /// The transformation map to which to add the ignore.
        /// </param>
        /// <param name="memberSelectors">A collection of functions, each selecting a top-level member to ignore.</param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException">A member of <paramref name="memberSelectors"/> is malformed.</exception>
        public static ITransformationMap<TCollection, TElement> Ignore<TCollection, TElement>(
            this ITransformationMap<TCollection, TElement> transformationMap,
            params Expression<Func<TCollection, object>>[] memberSelectors)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (transformationMap is not { } tm)
            {
                throw new ArgumentNullException(nameof(transformationMap));
            }

            for (var i = 0; i < memberSelectors.Length; i++)
            {
                if (TransformationMap.Builder<TCollection, TElement>.GetSelectorName(memberSelectors[i]) is not { } name)
                {
                    throw new ArgumentException(MalformedValueSelector, nameof(memberSelectors))
                    {
                        Data =
                        {
                            ["selector"] = memberSelectors[i],
                        },
                    };
                }

                _ = tm.Ignore(name);
            }

            return tm;
        }
    }
}
