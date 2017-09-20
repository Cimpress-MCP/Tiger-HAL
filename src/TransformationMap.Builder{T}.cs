﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <content>The builder for further transformations beyond "self".</content>
    public sealed partial class TransformationMap
    {
        /// <summary>Configures a created transformation map.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        [PublicAPI]
        internal class Builder<T>
            : ITransformationInstructions, ITransformationMap<T>
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
            public Builder([NotNull] Func<T, LinkBuilder> selfSelector)
            {
                _links["self"] = new LinkInstruction<T>(selfSelector);
            }

            /// <inheritdoc/>
            IReadOnlyDictionary<string, ILinkInstruction> ITransformationInstructions.LinkInstructions => _links;

            /// <inheritdoc/>
            IReadOnlyCollection<IEmbedInstruction> ITransformationInstructions.EmbedInstructions => _embeds;

            /// <inheritdoc/>
            IReadOnlyCollection<IHoistInstruction> ITransformationInstructions.HoistInstructions => _hoists;

            /// <inheritdoc/>
            IReadOnlyCollection<string> ITransformationInstructions.IgnoreInstructions => _ignores;

            /* todo(cosborn)
             * Should expressions allow indexing, in the case of collections and dictionaries?
             */

            #region Link

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Link(
                string relation,
                Func<T, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                _links[relation] = new LinkInstruction<T>(linkSelector);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Link<TMember>(
                string relation,
                Func<T, IEnumerable<TMember>> collectionSelector,
                Func<TMember, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }
                if (linkSelector == null) { throw new ArgumentNullException(nameof(linkSelector)); }

                _links[relation] = new ManyLinkInstruction<T>(t => collectionSelector(t).Select(linkSelector));
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Link<TMember>(
                string relation,
                Func<T, IEnumerable<TMember>> collectionSelector,
                Func<T, TMember, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (collectionSelector == null) { throw new ArgumentNullException(nameof(collectionSelector)); }

                _links[relation] = new ManyLinkInstruction<T>(t => collectionSelector(t).Select(tm => linkSelector(t, tm)));
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Link<TKey, TValue>(
                string relation,
                Func<T, IDictionary<TKey, TValue>> dictionarySelector,
                Func<TKey, TValue, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (dictionarySelector == null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

                _links[relation] = new ManyLinkInstruction<T>(
                    t => dictionarySelector(t).Select(kvp => linkSelector(kvp.Key, kvp.Value)));
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Link<TKey, TValue>(
                string relation,
                Func<T, IDictionary<TKey, TValue>> dictionarySelector,
                Func<T, TKey, TValue, LinkBuilder> linkSelector)
            {
                if (relation == null) { throw new ArgumentNullException(nameof(relation)); }
                if (dictionarySelector == null) { throw new ArgumentNullException(nameof(dictionarySelector)); }

                _links[relation] = new ManyLinkInstruction<T>(
                    t => dictionarySelector(t).Select(
                        kvp => linkSelector(t, kvp.Key, kvp.Value)));
                return this;
            }

            #endregion

            #region Embed

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Embed<TMember>(
                string relation,
                Expression<Func<T, TMember>> memberSelector,
                Func<T, LinkBuilder> linkSelector)
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

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Embed<TMember>(
                string relation,
                Expression<Func<T, TMember>> memberSelector,
                Func<T, TMember, LinkBuilder> linkSelector)
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

            #endregion

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Hoist<TMember>(Expression<Func<T, TMember>> memberSelector)
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

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(string memberSelector1)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }

                _ignores.Add(memberSelector1);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(string memberSelector1, string memberSelector2)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 == null) { throw new ArgumentNullException(nameof(memberSelector2)); }

                _ignores.Add(memberSelector1);
                _ignores.Add(memberSelector2);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(
                string memberSelector1,
                string memberSelector2,
                string memberSelector3)
            {
                if (memberSelector1 == null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 == null) { throw new ArgumentNullException(nameof(memberSelector2)); }
                if (memberSelector3 == null) { throw new ArgumentNullException(nameof(memberSelector3)); }

                _ignores.Add(memberSelector1);
                _ignores.Add(memberSelector2);
                _ignores.Add(memberSelector3);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(params string[] memberSelectors)
            {
                if (memberSelectors == null) { throw new ArgumentNullException(nameof(memberSelectors)); }

                _ignores.AddRange(memberSelectors);
                return this;
            }

            #endregion
        }
    }
}
