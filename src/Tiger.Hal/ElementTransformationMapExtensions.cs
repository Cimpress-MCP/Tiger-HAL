// <copyright file="ElementTransformationMapExtensions.cs" company="Cimpress, Inc.">
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
using Tiger.Types;
using static Tiger.Hal.LinkData;
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <summary>
    /// Extensions to the functionality of the <see cref="IElementTransformationMap{TCollection, TElement}"/> interface.
    /// </summary>
    public static class ElementTransformationMapExtensions
    {
        /// <summary>Creates links to the elements for the given collection type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="elementTransformationMap">The element transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        public static ITransformationMap<TCollection, TElement> LinkElements<TCollection, TElement>(
            this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            string relation,
            Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement> => elementTransformationMap is null
                ? throw new ArgumentNullException(nameof(elementTransformationMap))
                : elementTransformationMap.LinkElements(relation, t => selector(t).Pipe(Const));

        /// <summary>Creates links to the elements for the given collection type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="elementTransformationMap">The element transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> LinkElements<TCollection, TElement>(
            this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            Uri relation,
            Func<TElement, ILinkData?> selector)
            where TCollection : IReadOnlyCollection<TElement> => (elementTransformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(elementTransformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } etm, { AbsoluteUri: { } u }) => etm.LinkElements(u, selector),
            };

        /// <summary>Creates links to the elements for the given collection type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="elementTransformationMap">The element transformation map to which to add the link.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> LinkElements<TCollection, TElement>(
            this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            Uri relation,
            Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement> => (elementTransformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(elementTransformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } etm, { AbsoluteUri: { } u }) => etm.LinkElements(u, selector),
            };

        /// <summary>Creates embeds of the elements for the given collection type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="elementTransformationMap">The element transformation map to which to add the embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        public static ITransformationMap<TCollection, TElement> EmbedElements<TCollection, TElement>(
            this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            string relation,
            Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement> => elementTransformationMap is null
                ? throw new ArgumentNullException(nameof(elementTransformationMap))
                : elementTransformationMap.EmbedElements(relation, t => selector(t).Pipe(Const));

        /// <summary>Creates embeds of the elements for the given collection type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="elementTransformationMap">The element transformation map to which to add the embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> EmbedElements<TCollection, TElement>(
            this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            Uri relation,
            Func<TElement, ILinkData?> selector)
            where TCollection : IReadOnlyCollection<TElement> => (elementTransformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(elementTransformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } etm, { AbsoluteUri: { } u }) => etm.EmbedElements(u, selector),
            };

        /// <summary>Creates embeds of the elements for the given collection type.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
        /// <param name="elementTransformationMap">The element transformation map to which to add the embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="Uri"/>
        /// from a value of type <typeparamref name="TElement"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        public static ITransformationMap<TCollection, TElement> EmbedElements<TCollection, TElement>(
            this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            Uri relation,
            Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement> => (elementTransformationMap, relation) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(elementTransformationMap)),
                (_, null) => throw new ArgumentNullException(nameof(relation)),
                (_, { IsAbsoluteUri: false }) => throw new ArgumentException(RelativeRelationUri, nameof(relation)),
                ({ } etm, { AbsoluteUri: { } u }) => etm.EmbedElements(u, selector),
            };
    }
}
