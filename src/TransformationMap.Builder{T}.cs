using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <content>The builder for further transformations beyond "self".</content>
    public sealed partial class TransformationMap
    {
        /// <summary>Configures a created transformation map.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        [PublicAPI]
        public class Builder<T>
            : ITransformationMap
        {
            readonly Dictionary<string, ILinkInstruction> _links = new Dictionary<string, ILinkInstruction>();
            readonly List<IEmbedInstruction> _embeds = new List<IEmbedInstruction>();
            readonly List<IHoistInstruction> _hoists = new List<IHoistInstruction>();
            readonly List<string> _ignores = new List<string>();

            /// <summary>Initializes a new instance of the <see cref="Builder{T}"/> class.</summary>
            /// <param name="selfSelector">
            /// A function that creates a <see cref="LinkBuilder"/>
            /// from a value of type <typeparamref name="T"/>.
            /// </param>
            internal Builder([NotNull] Func<T, LinkBuilder> selfSelector)
            {
                _links["self"] = new LinkInstruction<T>(selfSelector);
            }

            /// <inheritdoc/>
            IReadOnlyDictionary<string, ILinkInstruction> ITransformationMap.LinkInstructions => _links;

            /// <inheritdoc/>
            IReadOnlyCollection<IEmbedInstruction> ITransformationMap.EmbedInstructions => _embeds;

            /// <inheritdoc/>
            IReadOnlyCollection<IHoistInstruction> ITransformationMap.HoistInstructions => _hoists;

            /// <inheritdoc/>
            IReadOnlyCollection<string> ITransformationMap.IgnoreInstructions => _ignores;

            /* todo(cosborn)
             * Should expressions allow indexing, in the case of collections and dictionaries?
             */

            #region Link

            /// <summary>Creates a link for the given type.</summary>
            /// <param name="relation">The name of the link relation to establish.</param>
            /// <param name="linkSelector">
            /// A function that creates a <see cref="LinkBuilder"/>
            /// from a value of type <typeparamref name="T"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link(
                [NotNull] string relation,
                [NotNull] Func<T, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                _links[relation] = new LinkInstruction<T>(linkSelector);
                return this;
            }

            /// <summary>Creates a link for the given type.</summary>
            /// <param name="relation">The name of the link relation to establish.</param>
            /// <param name="linkSelector">
            /// A function that creates a <see cref="LinkBuilder"/>
            /// from a value of type <typeparamref name="T"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link(
                [NotNull] Uri relation,
                [NotNull] Func<T, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                _links[relation.AbsoluteUri] = new LinkInstruction<T>(linkSelector);
                return this;
            }

            /// <summary>Creates a collection of links for the given type.</summary>
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
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TMember>(
                [NotNull] string relation,
                [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
                [NotNull] Func<TMember, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                _links[relation] = new ManyLinkInstruction<T>(t => collectionSelector(t).Select(linkSelector));
                return this;
            }

            /// <summary>Creates a collection of links for the given type.</summary>
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
            /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TMember>(
                [NotNull] Uri relation,
                [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
                [NotNull] Func<TMember, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
                if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                _links[relation.AbsoluteUri] = new ManyLinkInstruction<T>(t => collectionSelector(t).Select(linkSelector));
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
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TMember>(
                [NotNull] string relation,
                [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
                [NotNull] Func<T, TMember, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }

                _links[relation] = new ManyLinkInstruction<T>(t => collectionSelector(t).Select(tm => linkSelector(t, tm)));
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
            /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="collectionSelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TMember>(
                [NotNull] Uri relation,
                [NotNull] Func<T, IEnumerable<TMember>> collectionSelector,
                [NotNull] Func<T, TMember, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
                if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }

                _links[relation.AbsoluteUri] = new ManyLinkInstruction<T>(t => collectionSelector(t).Select(tm => linkSelector(t, tm)));
                return this;
            }

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
            /// A function that creates a <see cref="LinkBuilder"/> from a value of type
            /// <typeparamref name="TKey"/> and a value of type <typeparamref name="TValue"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TKey, TValue>(
                [NotNull] string relation,
                [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
                [NotNull] Func<TKey, TValue, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (dictionarySelector == null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

                _links[relation] = new ManyLinkInstruction<T>(
                    t => dictionarySelector(t).Select(kvp => linkSelector(kvp.Key, kvp.Value)));
                return this;
            }

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
            /// A function that creates a <see cref="LinkBuilder"/> from a value of type
            /// <typeparamref name="TKey"/> and a value of type <typeparamref name="TValue"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TKey, TValue>(
                [NotNull] Uri relation,
                [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
                [NotNull] Func<TKey, TValue, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
                if (dictionarySelector == null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

                _links[relation.AbsoluteUri] = new ManyLinkInstruction<T>(
                    t => dictionarySelector(t).Select(kvp => linkSelector(kvp.Key, kvp.Value)));
                return this;
            }

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
            /// A function that creates a <see cref="LinkBuilder"/> from a value of type
            /// <typeparamref name="T"/>, a value of type <typeparamref name="TKey"/>,
            /// and a value of type <typeparamref name="TValue"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TKey, TValue>(
                [NotNull] string relation,
                [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
                [NotNull] Func<T, TKey, TValue, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (dictionarySelector == null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

                _links[relation] = new ManyLinkInstruction<T>(
                    t => dictionarySelector(t).Select(
                        kvp => linkSelector(t, kvp.Key, kvp.Value)));
                return this;
            }

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
            /// A function that creates a <see cref="LinkBuilder"/> from a value of type
            /// <typeparamref name="T"/>, a value of type <typeparamref name="TKey"/>,
            /// and a value of type <typeparamref name="TValue"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="dictionarySelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Link<TKey, TValue>(
                [NotNull] Uri relation,
                [NotNull] Func<T, IDictionary<TKey, TValue>> dictionarySelector,
                [NotNull] Func<T, TKey, TValue, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
                if (dictionarySelector == null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

                _links[relation.AbsoluteUri] = new ManyLinkInstruction<T>(
                    t => dictionarySelector(t).Select(
                        kvp => linkSelector(t, kvp.Key, kvp.Value)));
                return this;
            }

            #endregion

            #region Embed

            /// <summary>Creates an embed for the given type, using only the main object.</summary>
            /// <typeparam name="TMember">The type of the selected value.</typeparam>
            /// <param name="relation">The name of the link relation to establish.</param>
            /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
            /// <param name="linkSelector">
            /// A function that creates a <see cref="LinkBuilder"/>
            /// from a value of type <typeparamref name="T"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Embed<TMember>(
                [NotNull] string relation,
                [NotNull] Expression<Func<T, TMember>> memberSelector,
                [NotNull] Func<T, LinkBuilder> linkSelector)
            {
                if (memberSelector == null) { throw new ArgumentNullException(nameof(memberSelector)); }
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                switch (memberSelector.Body)
                {
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        _links[relation] = new LinkInstruction<T>(linkSelector);
                        _embeds.Add(new MemberEmbedInstruction<T, TMember>(relation, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <summary>Creates an embed for the given type, using only the main object.</summary>
            /// <typeparam name="TMember">The type of the selected value.</typeparam>
            /// <param name="relation">The name of the link relation to establish.</param>
            /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
            /// <param name="linkSelector">
            /// A function that creates a <see cref="LinkBuilder"/>
            /// from a value of type <typeparamref name="T"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="relation"/> is not an absolute <see cref="Uri"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Embed<TMember>(
                [NotNull] Uri relation,
                [NotNull] Expression<Func<T, TMember>> memberSelector,
                [NotNull] Func<T, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
                if (memberSelector == null) { throw new ArgumentNullException(nameof(memberSelector)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                switch (memberSelector.Body)
                {
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        _links[relation.AbsoluteUri] = new LinkInstruction<T>(linkSelector);
                        _embeds.Add(new MemberEmbedInstruction<T, TMember>(relation.AbsoluteUri, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <summary>Creates an embed for the given type, using the main object and the selected object.</summary>
            /// <typeparam name="TMember">The type of the selected property.</typeparam>
            /// <param name="relation">The name of the link relation to establish.</param>
            /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
            /// <param name="linkSelector">
            /// A function that creates a <see cref="LinkBuilder"/> from a value of type
            /// <typeparamref name="T"/> and a value of type <typeparamref name="TMember"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Embed<TMember>(
                [NotNull] string relation,
                [NotNull] Expression<Func<T, TMember>> memberSelector,
                [NotNull] Func<T, TMember, LinkBuilder> linkSelector)
            {
                if (memberSelector == null) { throw new ArgumentNullException(nameof(memberSelector)); }
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                switch (memberSelector.Body)
                { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        _links[relation] = new LinkInstruction<T>(t => linkSelector(t, valueSelector(t)));
                        _embeds.Add(new MemberEmbedInstruction<T, TMember>(relation, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <summary>Creates an embed for the given type, using the main object and the selected object.</summary>
            /// <typeparam name="TMember">The type of the selected property.</typeparam>
            /// <param name="relation">The name of the link relation to establish.</param>
            /// <param name="memberSelector">A function selecting a top-level member to embed.</param>
            /// <param name="linkSelector">
            /// A function that creates a <see cref="LinkBuilder"/> from a value of type
            /// <typeparamref name="T"/> and a value of type <typeparamref name="TMember"/>.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="memberSelector"/> is malformed.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="linkSelector"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Embed<TMember>(
                [NotNull] Uri relation,
                [NotNull] Expression<Func<T, TMember>> memberSelector,
                [NotNull] Func<T, TMember, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (!relation.IsAbsoluteUri) { throw new ArgumentException(RelativeRelationUri, nameof(relation)); }
                if (memberSelector == null) { throw new ArgumentNullException(nameof(memberSelector)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                switch (memberSelector.Body)
                { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        _links[relation.AbsoluteUri] = new LinkInstruction<T>(t => linkSelector(t, valueSelector(t)));
                        _embeds.Add(new MemberEmbedInstruction<T, TMember>(relation.AbsoluteUri, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            #endregion

            /// <summary>
            /// Hoists a property that would not be present in the HAL representation of an array value
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
            public Builder<T> Hoist<TMember>([NotNull] Expression<Func<T, TMember>> memberSelector)
            {
                if (memberSelector == null) { throw new ArgumentNullException(nameof(memberSelector)); }

                switch (memberSelector.Body)
                {
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        _hoists.Add(new MemberHoistInstruction<T, TMember>(me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            #region Ignore

            /// <summary>Causes a member not to be represented in the HAL+JSON serialization of a value.</summary>
            /// <param name="memberSelector1">
            /// The name of a top-level member of type <typeparamref name="T"/> to ignore.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Ignore([NotNull, InvokerParameterName] string memberSelector1)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }

                _ignores.Add(memberSelector1);
                return this;
            }

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
            public Builder<T> Ignore(
                [NotNull, InvokerParameterName] string memberSelector1,
                [NotNull, InvokerParameterName] string memberSelector2)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 == null) { throw new ArgumentNullException(nameof(memberSelector2)); }

                _ignores.Add(memberSelector1);
                _ignores.Add(memberSelector2);
                return this;
            }

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
            public Builder<T> Ignore(
                [NotNull, InvokerParameterName] string memberSelector1,
                [NotNull, InvokerParameterName] string memberSelector2,
                [NotNull, InvokerParameterName] string memberSelector3)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 == null) { throw new ArgumentNullException(nameof(memberSelector2)); }
                if (memberSelector3 == null) { throw new ArgumentNullException(nameof(memberSelector3)); }

                _ignores.Add(memberSelector1);
                _ignores.Add(memberSelector2);
                _ignores.Add(memberSelector3);
                return this;
            }

            /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
            /// <param name="memberSelectors">
            /// A collection of top-level members of type <typeparamref name="T"/> to ignore.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelectors"/> is <see langword="null"/>.</exception>
            [NotNull]
            public Builder<T> Ignore(
                [NotNull, InvokerParameterName] params string[] memberSelectors)
            {
                if (memberSelectors == null) { throw new ArgumentNullException(nameof(memberSelectors)); }

                _ignores.AddRange(memberSelectors);
                return this;
            }

            /// <summary>Causes a member not to be represented in the HAL+JSON serialization of a value.</summary>
            /// <typeparam name="T1">The type of the member selected by <paramref name="memberSelector1"/>.</typeparam>
            /// <param name="memberSelector1">
            /// A function selecting a top-level member of type <typeparamref name="T1"/> to ignore.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
            [NotNull]
            public Builder<T> Ignore<T1>([NotNull] Expression<Func<T, T1>> memberSelector1)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }

                switch (memberSelector1.Body)
                {
                    case MemberExpression me:
                        _ignores.Add(me.Member.Name);
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
                }
            }

            /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
            /// <typeparam name="T1">The type of the member selected by <paramref name="memberSelector1"/>.</typeparam>
            /// <typeparam name="T2">The type of the member selected by <paramref name="memberSelector2"/>.</typeparam>
            /// <param name="memberSelector1">
            /// A function selecting a top-level member of type <typeparamref name="T1"/> to ignore.
            /// </param>
            /// <param name="memberSelector2">
            /// A function selecting a top-level member of type <typeparamref name="T2"/> to ignore.
            /// </param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector2"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
            [NotNull]
            public Builder<T> Ignore<T1, T2>(
                [NotNull] Expression<Func<T, T1>> memberSelector1,
                [NotNull] Expression<Func<T, T2>> memberSelector2)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 == null) { throw new ArgumentNullException(nameof(memberSelector2)); }

                switch (memberSelector1.Body)
                {
                    case MemberExpression me:
                        _ignores.Add(me.Member.Name);
                        break;
                    default:
                        throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
                }

                switch (memberSelector2.Body)
                { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                    case MemberExpression me:
                        _ignores.Add(me.Member.Name);
                        break;
                    default:
                        throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2));
                }

                return this;
            }

            /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
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
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector1"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">A member of <paramref name="memberSelector1"/> is malformed.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector2"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">A member of <paramref name="memberSelector2"/> is malformed.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelector3"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">A member of <paramref name="memberSelector3"/> is malformed.</exception>
            [NotNull]
            public Builder<T> Ignore<T1, T2, T3>(
                [NotNull] Expression<Func<T, T1>> memberSelector1,
                [NotNull] Expression<Func<T, T2>> memberSelector2,
                [NotNull] Expression<Func<T, T3>> memberSelector3)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 == null) { throw new ArgumentNullException(nameof(memberSelector2)); }
                if (memberSelector3 == null) { throw new ArgumentNullException(nameof(memberSelector3)); }

                switch (memberSelector1.Body)
                {
                    case MemberExpression me:
                        _ignores.Add(me.Member.Name);
                        break;
                    default:
                        throw new ArgumentException(MalformedValueSelector, nameof(memberSelector1));
                }

                switch (memberSelector2.Body)
                {
                    case MemberExpression me:
                        _ignores.Add(me.Member.Name);
                        break;
                    default:
                        throw new ArgumentException(MalformedValueSelector, nameof(memberSelector2));
                }

                switch (memberSelector3.Body)
                {
                    case MemberExpression me:
                        _ignores.Add(me.Member.Name);
                        break;
                    default:
                        throw new ArgumentException(MalformedValueSelector, nameof(memberSelector3));
                }

                return this;
            }

            /// <summary>Causes members not to be represented in the HAL+JSON serialization of a value.</summary>
            /// <param name="memberSelectors">A collection of functions, each selecting a top-level member to ignore.</param>
            /// <returns>The modified transformation map.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="memberSelectors"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">A member of <paramref name="memberSelectors"/> is malformed.</exception>
            [NotNull]
            public Builder<T> Ignore([NotNull, ItemNotNull] params Expression<Func<T, object>>[] memberSelectors)
            {
                if (memberSelectors == null) { throw new ArgumentNullException(nameof(memberSelectors)); }

                void SelectMember(Expression e)
                {
                    switch (e)
                    {
                        case UnaryExpression ue when ue.NodeType == ExpressionType.Convert:
                            /* note(cosborn)
                             * Because we have to fall back to Expression<Func<T, object>>,
                             * value types will be wrapped in a Convert call by the compiler.
                             */
                            SelectMember(ue.Operand);
                            break;
                        case MemberExpression me:
                            _ignores.Add(me.Member.Name);
                            break;
                        default:
                            throw new ArgumentException(MalformedValueSelector, nameof(memberSelectors))
                            {
                                Data =
                                {
                                    ["selector"] = e
                                }
                            };
                    }
                }

                foreach (var memberSelector in memberSelectors)
                {
                    SelectMember(memberSelector.Body);
                }

                return this;
            }

            #endregion
        }
    }
}
