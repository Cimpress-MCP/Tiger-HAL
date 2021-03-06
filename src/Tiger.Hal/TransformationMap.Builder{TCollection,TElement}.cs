// <copyright file="TransformationMap.Builder{TCollection,TElement}.cs" company="Cimpress, Inc.">
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
using static Tiger.Hal.Properties.Resources;

namespace Tiger.Hal
{
    /// <summary>The builder for further transformations beyond "self".</summary>
    sealed partial class TransformationMap
    {
        /// <summary>Configures a created transformation map.</summary>
        /// <typeparam name="TCollection">The collection type being transformed.</typeparam>
        /// <typeparam name="TElement">The element type of the collection type being transformed.</typeparam>
        internal class Builder<TCollection, TElement>
            : Builder<TCollection>, ITransformationInstructions, IElementTransformationMap<TCollection, TElement>, ITransformationMap<TCollection, TElement>
            where TCollection : IReadOnlyCollection<TElement>
        {
            readonly List<IHoistInstruction> _hoists = new();

            /// <summary>Initializes a new instance of the <see cref="Builder{TCollection, TElement}"/> class.</summary>
            /// <param name="self">
            /// A function that creates an <see cref="ILinkData"/>
            /// from a value of type <typeparamref name="TCollection"/>.
            /// </param>
            public Builder(Func<TCollection, ILinkData?> self)
                : base(self)
            {
            }

            /// <inheritdoc/>
            IReadOnlyDictionary<string, ILinkInstruction> ITransformationInstructions.LinkInstructions => Links;

            /// <inheritdoc/>
            IReadOnlyCollection<IEmbedInstruction> ITransformationInstructions.EmbedInstructions => Embeds;

            /// <inheritdoc/>
            IReadOnlyCollection<IHoistInstruction> ITransformationInstructions.HoistInstructions => _hoists;

            /// <inheritdoc/>
            IReadOnlyCollection<string> ITransformationInstructions.IgnoreInstructions => Ignores;

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Hoist<TMember>(Expression<Func<TCollection, TMember>> selector)
            {
                var name = GetSelectorName(selector);
                if (name is null)
                {
                    throw new ArgumentException(MalformedValueSelector, nameof(selector));
                }

                var valueSelector = selector.Compile();
                _hoists.Add(new MemberHoistInstruction<TCollection, TMember>(name, valueSelector));
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> IElementTransformationMap<TCollection, TElement>.LinkElements(
                string relation,
                Func<TElement, ILinkData?> selector)
            {
                Links[relation] = new ManyLinkInstruction<TCollection>(c => c.Select(selector));
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> IElementTransformationMap<TCollection, TElement>.EmbedElements(
                string relation,
                Func<TElement, ILinkData?> selector)
            {
                Links[relation] = new ManyLinkInstruction<TCollection>(c => c.Select(selector));
                Embeds.Add(new ManyEmbedInstruction<TCollection, TElement>(relation, "[*]", _ => _));

                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Link(
                string relation,
                Func<TCollection, ILinkData?> selector)
            {
                Links[relation] = new LinkInstruction<TCollection>(selector);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Link<TMember>(
                string relation,
                Func<TCollection, IEnumerable<TMember>> collectionSelector,
                Func<TCollection, TMember, ILinkData?> linkSelector)
            {
                Links[relation] = new ManyLinkInstruction<TCollection>(t => collectionSelector(t).Select(tm => linkSelector(t, tm)));
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Embed<TMember>(
                string relation,
                Expression<Func<TCollection, TMember>> memberSelector,
                Func<TCollection, ILinkData?> linkSelector)
            {
                switch (memberSelector.Body)
                {
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        Links[relation] = new LinkInstruction<TCollection>(linkSelector);
                        Embeds.Add(new MemberEmbedInstruction<TCollection, TMember>(relation, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Embed<TMember>(
                string relation,
                Expression<Func<TCollection, TMember>> memberSelector,
                Func<TCollection, TMember, ILinkData?> linkSelector)
            {
                switch (memberSelector.Body)
                { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        Links[relation] = new LinkInstruction<TCollection>(t => linkSelector(t, valueSelector(t)));
                        Embeds.Add(new MemberEmbedInstruction<TCollection, TMember>(relation, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Ignore(string memberSelector1)
            {
                Ignores.Add(memberSelector1);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Ignore(string memberSelector1, string memberSelector2)
            {
                Ignores.Add(memberSelector1);
                Ignores.Add(memberSelector2);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Ignore(
                string memberSelector1,
                string memberSelector2,
                string memberSelector3)
            {
                Ignores.Add(memberSelector1);
                Ignores.Add(memberSelector2);
                Ignores.Add(memberSelector3);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<TCollection, TElement> ITransformationMap<TCollection, TElement>.Ignore(params string[] memberSelectors)
            {
                Ignores.AddRange(memberSelectors);
                return this;
            }
        }
    }
}
