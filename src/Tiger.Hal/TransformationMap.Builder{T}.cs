// <copyright file="TransformationMap.Builder{T}.cs" company="Cimpress, Inc.">
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
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using static Tiger.Hal.Properties.Resources;
using static Tiger.Hal.Relations;

namespace Tiger.Hal
{
    /// <summary>The builder for further transformations beyond "self".</summary>
    sealed partial class TransformationMap
    {
        /// <summary>Configures a created transformation map.</summary>
        /// <typeparam name="T">The type being transformed.</typeparam>
        internal class Builder<T>
            : ITransformationInstructions, ITransformationMap<T>
        {
            /// <summary>Initializes a new instance of the <see cref="Builder{T}"/> class.</summary>
            /// <param name="self">
            /// A function that creates an <see cref="ILinkData"/>
            /// from a value of type <typeparamref name="T"/>.
            /// </param>
            public Builder(Func<T, ILinkData?> self)
            {
                Links[Self] = new LinkInstruction<T>(self);
            }

            /// <inheritdoc/>
            IReadOnlyDictionary<string, ILinkInstruction> ITransformationInstructions.LinkInstructions => Links;

            /// <inheritdoc/>
            IReadOnlyCollection<IEmbedInstruction> ITransformationInstructions.EmbedInstructions => Embeds;

            /// <inheritdoc/>
            IReadOnlyCollection<IHoistInstruction> ITransformationInstructions.HoistInstructions => ImmutableArray<IHoistInstruction>.Empty;

            /// <inheritdoc/>
            IReadOnlyCollection<string> ITransformationInstructions.IgnoreInstructions => Ignores;

            /// <summary>Gets the collection of link instructions.</summary>
            protected Dictionary<string, ILinkInstruction> Links { get; } = new Dictionary<string, ILinkInstruction>();

            /// <summary>Gets the collection of embed instructions.</summary>
            protected List<IEmbedInstruction> Embeds { get; } = new List<IEmbedInstruction>();

            /// <summary>Gets the collection of ignore instructions.</summary>
            protected List<string> Ignores { get; } = new List<string>();

            /// <summary>
            /// Gets the name of the selected property from <paramref name="selector"/>,
            /// if <paramref name="selector"/> represents a simple property selector.
            /// </summary>
            /// <typeparam name="TProperty">The return type of the selector.</typeparam>
            /// <param name="selector">The selector from wihch to get the name.</param>
            /// <returns>
            /// The name of the selected property, if <paramref name="selector"/> represents
            /// a simple property selector; otherwise, <see langword="null"/>.
            /// </returns>
            public static string? GetSelectorName<TProperty>(Expression<Func<T, TProperty>> selector)
            {
                var parameter = selector.Parameters[0];

                return GetIgnoreNameCore(parameter.Name, selector.Body);

                static string? GetIgnoreNameCore(string name, Expression body) => body switch
                {
                    MemberExpression { Expression: ParameterExpression { Name: { } paramName }, Member: { Name: { } memberName } } when paramName == name => memberName,
                    UnaryExpression { NodeType: ExpressionType.Convert, Operand: { } op } => GetIgnoreNameCore(name, op),
                    _ => null,
                };
            }

            /* todo(cosborn)
             * Should expressions allow indexing, in the case of collections and dictionaries?
             */

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Link(string relation, ILinkInstruction instruction)
            {
                Links[relation] = instruction;
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.EmbedElements<TElement>(
                string relation,
                Expression<Func<T, IReadOnlyCollection<TElement>>> collectionSelector,
                Func<TElement, ILinkData?> linkSelector)
            {
                switch (collectionSelector.Body)
                {
                    case MemberExpression { Member: { Name: { } n } }:
                        var valueSelector = collectionSelector.Compile();
                        Links[relation] = new ManyLinkInstruction<T>(c => valueSelector(c).Select(linkSelector));
                        Embeds.Add(new ManyEmbedInstruction<T, TElement>(relation, n, t => valueSelector(t)));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Embed<TMember>(
                string relation,
                Expression<Func<T, TMember>> memberSelector,
                Func<T, ILinkData?> linkSelector)
            {
                switch (memberSelector.Body)
                {
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        Links[relation] = new LinkInstruction<T>(linkSelector);
                        Embeds.Add(new MemberEmbedInstruction<T, TMember>(relation, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Embed<TMember>(
                string relation,
                Expression<Func<T, TMember>> memberSelector,
                Func<T, TMember, ILinkData?> linkSelector)
            {
                switch (memberSelector.Body)
                { // todo(cosborn) Allow indexing, in the case of collections and dictionaries?
                    case MemberExpression me:
                        var valueSelector = memberSelector.Compile();
                        Links[relation] = new LinkInstruction<T>(t => linkSelector(t, valueSelector(t)));
                        Embeds.Add(new MemberEmbedInstruction<T, TMember>(relation, me.Member.Name, valueSelector));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(string memberSelector1)
            {
                Ignores.Add(memberSelector1);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(string memberSelector1, string memberSelector2)
            {
                Ignores.Add(memberSelector1);
                Ignores.Add(memberSelector2);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(
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
            ITransformationMap<T> ITransformationMap<T>.Ignore(params string[] memberSelectors)
            {
                Ignores.AddRange(memberSelectors);
                return this;
            }
        }
    }
}
