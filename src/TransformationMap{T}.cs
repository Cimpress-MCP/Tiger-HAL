using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Defines a series of transformations for a type to its HAL representation.</summary>
    /// <typeparam name="T">The type being transformed.</typeparam>
    [PublicAPI]
    public sealed class TransformationMap<T>
        : ITransformationMap
    {
        readonly List<ILinkInstruction> _links = new List<ILinkInstruction>();
        readonly List<IEmbedInstruction> _embeds = new List<IEmbedInstruction>();

        /// <summary>Initializes a new instance of the <see cref="TransformationMap{T}"/> class.</summary>
        /// <param name="selfSelector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>
        /// which will be used to create the "self" link.
        /// </param>
        internal TransformationMap([NotNull] Func<T, LinkBuilder> selfSelector)
        {
            if (selfSelector == null) { throw new ArgumentNullException(nameof(selfSelector)); }

            _links.Add(new SimpleLinkInstruction<T>("self", selfSelector));
        }

        /// <inheritdoc/>
        IReadOnlyCollection<ILinkInstruction> ITransformationMap.LinkInstructions => _links;

        /// <inheritdoc/>
        IReadOnlyCollection<IEmbedInstruction> ITransformationMap.EmbedInstructions => _embeds;

        /// <summary>Creates a link for the given type.</summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="selector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public TransformationMap<T> Link(
            [NotNull] string relation,
            [NotNull] Func<T, LinkBuilder> selector)
        {
            if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
            if (selector == null) { throw new ArgumentNullException(nameof(selector)); }

            _links.Add(new SimpleLinkInstruction<T>(relation, selector));
            return this;
        }

        /// <summary>Creates an embed for the given type, using only the main object.</summary>
        /// <typeparam name="TSelected">The type of the selected value.</typeparam>
        /// <param name="valueSelector">A function that retrieves the value to embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Embed's value selector is malformed; it may only refer to a first-level member.</exception>
        [NotNull]
        public TransformationMap<T> Embed<TSelected>(
            [NotNull] Expression<Func<T, TSelected>> valueSelector,
            [NotNull] string relation,
            [NotNull] Func<T, LinkBuilder> linkSelector)
        {
            if (valueSelector == null) { throw new ArgumentNullException(nameof(valueSelector)); }
            if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
            if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

            // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
            switch (valueSelector.Body)
            {
                case MemberExpression me:
                    var propertySelector = valueSelector.Compile();
                    _links.Add(new SimpleLinkInstruction<T>(relation, linkSelector));
                    _embeds.Add(new MemberEmbedInstruction<T, TSelected>(relation, me.Member.Name, propertySelector));
                    return this;
                default:
                    throw new ArgumentException("Embed's value selector is malformed; it may only refer to a first-level member.");
            }
        }

        /// <summary>
        /// Creates an embed for the given type, using the main object and the selected object.
        /// </summary>
        /// <typeparam name="TSelected">The type of the selected property.</typeparam>
        /// <param name="valueSelector">A function that retrieves the value to embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type
        /// <typeparamref name="T"/> and a value of type <typeparamref name="TSelected"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Embed's value selector is malformed; it may only refer to a first-level member.</exception>
        [NotNull]
        public TransformationMap<T> Embed<TSelected>(
            [NotNull] Expression<Func<T, TSelected>> valueSelector,
            [NotNull] string relation,
            [NotNull] Func<T, TSelected, LinkBuilder> linkSelector)
        {
            if (valueSelector == null) { throw new ArgumentNullException(nameof(valueSelector)); }
            if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
            if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

            switch (valueSelector.Body)
            { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                case MemberExpression me:
                    var propertySelector = valueSelector.Compile();
                    _links.Add(new MemberLinkInstruction<T, TSelected>(relation, propertySelector, linkSelector));
                    _embeds.Add(new MemberEmbedInstruction<T, TSelected>(relation, me.Member.Name, propertySelector));
                    return this;
                default:
                    throw new ArgumentException("Embed's value selector is malformed; it may only refer to a first-level member.");
            }
        }
    }
}
