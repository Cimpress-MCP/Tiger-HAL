using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <summary>Defines a series of transformations for a type to its HAL representation.</summary>
    /// <typeparam name="T">The type being transformed.</typeparam>
    [PublicAPI]
    public sealed class TransformationMap<T>
        : ITransformationMap
    {
        readonly Dictionary<string, ILinkInstruction> _links = new Dictionary<string, ILinkInstruction>();
        readonly List<IEmbedInstruction> _embeds = new List<IEmbedInstruction>();
        readonly List<IHoistInstruction> _hoists = new List<IHoistInstruction>();

        /// <summary>Initializes a new instance of the <see cref="TransformationMap{T}"/> class.</summary>
        /// <param name="selfSelector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>
        /// which will be used to create the "self" link.
        /// </param>
        internal TransformationMap([NotNull] Func<T, LinkBuilder> selfSelector)
        {
            if (selfSelector == null) { throw new ArgumentNullException(nameof(selfSelector)); }

            _links["self"] = new SimpleLinkInstruction<T>(selfSelector);
        }

        /// <inheritdoc/>
        IReadOnlyDictionary<string, ILinkInstruction> ITransformationMap.LinkInstructions => _links;

        /// <inheritdoc/>
        IReadOnlyCollection<IEmbedInstruction> ITransformationMap.EmbedInstructions => _embeds;

        /// <inheritdoc/>
        IReadOnlyCollection<IHoistInstruction> ITransformationMap.HoistInstructions => _hoists;

        /// <summary>Creates a collection of links for the given type.</summary>
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

            _links[relation] = new SimpleLinkInstruction<T>(selector);
            return this;
        }

        /// <summary>Creates a link for the given type.</summary>
        /// <typeparam name="TMember">
        /// The member type of the return type of <paramref name="collectionSelector"/>.
        /// </typeparam>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="collectionSelector">
        /// A function that selects a collection from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public TransformationMap<T> Link<TMember>(
            [NotNull] string relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<TMember, LinkBuilder> linkSelector)
        {
            if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
            if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }

            _links[relation] = new PluralSimpleLinkInstruction<T>(t => collectionSelector(t).Select(linkSelector));
            return this;
        }

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
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
        [NotNull]
        public TransformationMap<T> Link<TMember>(
            [NotNull] string relation,
            [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
            [NotNull] Func<T, TMember, LinkBuilder> linkSelector)
        {
            if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
            if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }

            _links[relation] = new PluralSimpleLinkInstruction<T>(
                t => collectionSelector(t).Select(tm => linkSelector(t, tm)));
            return this;
        }

        /// <summary>Creates an embed for the given type, using only the main object.</summary>
        /// <typeparam name="TMember">The type of the selected value.</typeparam>
        /// <param name="valueSelector">A function selecting a top-level member to embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/>
        /// from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="valueSelector"/> is malformed; it may only refer to a first-level member.</exception>
        [NotNull]
        public TransformationMap<T> Embed<TMember>(
            [NotNull] Expression<Func<T, TMember>> valueSelector,
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
                    var memberSelector = valueSelector.Compile();
                    _links[relation] = new SimpleLinkInstruction<T>(linkSelector);
                    _embeds.Add(new MemberEmbedInstruction<T, TMember>(relation, me.Member.Name, memberSelector));
                    return this;
                default:
                    throw new ArgumentException("Embed's value selector is malformed; it may only refer to a first-level member.");
            }
        }

        /// <summary>Creates an embed for the given type, using the main object and the selected object.</summary>
        /// <typeparam name="TMember">The type of the selected property.</typeparam>
        /// <param name="valueSelector">A function selecting a top-level member to embed.</param>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="linkSelector">
        /// A function that creates a <see cref="LinkBuilder"/> from a value of type
        /// <typeparamref name="T"/> and a value of type <typeparamref name="TMember"/>.
        /// </param>
        /// <returns>The modified transformation map.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="valueSelector"/> is malformed</exception>
        [NotNull]
        public TransformationMap<T> Embed<TMember>(
            [NotNull] Expression<Func<T, TMember>> valueSelector,
            [NotNull] string relation,
            [NotNull] Func<T, TMember, LinkBuilder> linkSelector)
        {
            if (valueSelector == null) { throw new ArgumentNullException(nameof(valueSelector)); }
            if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
            if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

            switch (valueSelector.Body)
            { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                case MemberExpression me:
                    var memberSelector = valueSelector.Compile();
                    _links[relation] = new MemberLinkInstruction<T, TMember>(memberSelector, linkSelector);
                    _embeds.Add(new MemberEmbedInstruction<T, TMember>(relation, me.Member.Name, memberSelector));
                    return this;
                default:
                    throw new ArgumentException(MalformedValueSelector);
            }
        }

        /// <summary>
        /// Hoists a property that would not be present in the HAL representation of an array value
        /// to the containing object value.
        /// </summary>
        /// <typeparam name="TMember">The type of the value to hoist.</typeparam>
        /// <param name="valueSelector">A function selecting a top-level member to hoist.</param>
        /// <returns>The modified transformation map.</returns>
        /// <remarks>
        /// This only has a meaningful effect on types which resolve to a <see cref="JsonArrayContract"/> --
        /// types which are not serialized as objects. Types which are serialized as objects already have
        /// their member keys at the root level.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="valueSelector"/> is malformed.</exception>
        [NotNull]
        public TransformationMap<T> Hoist<TMember>([NotNull] Expression<Func<T, TMember>> valueSelector)
        {
            if (valueSelector == null) { throw new ArgumentNullException(nameof(valueSelector)); }

            switch (valueSelector.Body)
            { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                case MemberExpression me:
                    var memberSelector = valueSelector.Compile();
                    _hoists.Add(new MemberHoistInstruction<T, TMember>(me.Member.Name, memberSelector));
                    return this;
                default:
                    throw new ArgumentException(MalformedValueSelector);
            }
        }
    }
}
