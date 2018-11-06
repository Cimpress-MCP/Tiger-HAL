// <copyright file="ElementTransformationMapExtensions.cs" company="Cimpress, Inc.">
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
        /// <exception cref="ArgumentNullException"><paramref name="elementTransformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> LinkElements<TCollection, TElement>(
            [NotNull] this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            [NotNull] string relation,
            [NotNull] Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (elementTransformationMap is null) { throw new ArgumentNullException(nameof(elementTransformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return elementTransformationMap.LinkElements(relation, t => selector(t)?.Pipe(Const));
        }

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
        /// <exception cref="ArgumentNullException"><paramref name="elementTransformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> LinkElements<TCollection, TElement>(
            [NotNull] this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TElement, ILinkData> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (elementTransformationMap is null) { throw new ArgumentNullException(nameof(elementTransformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return elementTransformationMap.LinkElements(relation.AbsoluteUri, selector);
        }

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
        /// <exception cref="ArgumentNullException"><paramref name="elementTransformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> LinkElements<TCollection, TElement>(
            [NotNull] this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (elementTransformationMap is null) { throw new ArgumentNullException(nameof(elementTransformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return elementTransformationMap.LinkElements(relation.AbsoluteUri, selector);
        }

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
        /// <exception cref="ArgumentNullException"><paramref name="elementTransformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> EmbedElements<TCollection, TElement>(
            [NotNull] this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            [NotNull] string relation,
            [NotNull] Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (elementTransformationMap is null) { throw new ArgumentNullException(nameof(elementTransformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return elementTransformationMap.EmbedElements(relation, t => selector(t)?.Pipe(Const));
        }

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
        /// <exception cref="ArgumentNullException"><paramref name="elementTransformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> EmbedElements<TCollection, TElement>(
            [NotNull] this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TElement, ILinkData> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (elementTransformationMap is null) { throw new ArgumentNullException(nameof(elementTransformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return elementTransformationMap.EmbedElements(relation.AbsoluteUri, selector);
        }

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
        /// <exception cref="ArgumentNullException"><paramref name="elementTransformationMap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public static ITransformationMap<TCollection, TElement> EmbedElements<TCollection, TElement>(
            [NotNull] this IElementTransformationMap<TCollection, TElement> elementTransformationMap,
            [NotNull] Uri relation,
            [NotNull] Func<TElement, Uri> selector)
            where TCollection : IReadOnlyCollection<TElement>
        {
            if (elementTransformationMap is null) { throw new ArgumentNullException(nameof(elementTransformationMap)); }
            if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
            if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
            if (selector is null) { throw new ArgumentNullException(nameof(selector)); }

            return elementTransformationMap.EmbedElements(relation.AbsoluteUri, selector);
        }
    }
}
