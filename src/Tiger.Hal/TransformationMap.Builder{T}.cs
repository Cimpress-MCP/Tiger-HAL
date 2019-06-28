// <copyright file="TransformationMap.Builder{T}.cs" company="Cimpress, Inc.">
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
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
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
            /// <exception cref="ArgumentNullException"><paramref name="self"/> is <see langword="null"/>.</exception>
            public Builder([NotNull] Func<T, ILinkData> self)
            {
                if (self is null) { throw new ArgumentNullException(nameof(self)); }

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
            public static string GetSelectorName<TProperty>(Expression<Func<T, TProperty>> selector)
            {
                var parameter = selector.Parameters[0];

                return GetIgnoreNameCore(parameter.Name, selector.Body);

                string GetIgnoreNameCore(string name, Expression body)
                {
                    switch (body)
                    {
                        case MemberExpression me when me.Expression is ParameterExpression pe && pe.Name == name:
                            return me.Member.Name;
                        case UnaryExpression ue when ue.NodeType == ExpressionType.Convert:
                            return GetIgnoreNameCore(name, ue.Operand);
                        default:
                            return null;
                    }
                }
            }

            /* todo(cosborn)
             * Should expressions allow indexing, in the case of collections and dictionaries?
             */

            #region Link

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Link(string relation, ILinkInstruction instruction)
            {
                if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
                if (instruction is null) { throw new ArgumentNullException(nameof(instruction)); }

                Links[relation] = instruction;
                return this;
            }

            #endregion

            #region Embed

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.EmbedElements<TElement>(
                string relation,
                Expression<Func<T, IReadOnlyCollection<TElement>>> collectionSelector,
                Func<TElement, ILinkData> linkSelector)
            {
                if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
                if (collectionSelector is null) { throw new ArgumentNullException(nameof(collectionSelector)); }
                if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

                switch (collectionSelector.Body)
                {
                    case MemberExpression me:
                        var valueSelector = collectionSelector.Compile();
                        Links[relation] = new ManyLinkInstruction<T>(c => valueSelector(c).Select(linkSelector));
                        Embeds.Add(new ManyEmbedInstruction<T, TElement>(relation, me.Member.Name, t => valueSelector(t)));
                        return this;
                    default:
                        throw new ArgumentException(MalformedValueSelector);
                }
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Embed<TMember>(
                string relation,
                Expression<Func<T, TMember>> memberSelector,
                Func<T, ILinkData> linkSelector)
            {
                if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
                if (memberSelector is null) { throw new ArgumentNullException(nameof(memberSelector)); }
                if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

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
                Func<T, TMember, ILinkData> linkSelector)
            {
                if (relation is null) { throw new ArgumentNullException(nameof(relation)); }
                if (memberSelector is null) { throw new ArgumentNullException(nameof(memberSelector)); }
                if (linkSelector is null) { throw new ArgumentNullException(nameof(linkSelector)); }

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

            #endregion

            #region Ignore

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(string memberSelector1)
            {
                if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }

                Ignores.Add(memberSelector1);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(string memberSelector1, string memberSelector2)
            {
                if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 is null) { throw new ArgumentNullException(nameof(memberSelector2)); }

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
                if (memberSelector1 is null) { throw new ArgumentNullException(nameof(memberSelector1)); }
                if (memberSelector2 is null) { throw new ArgumentNullException(nameof(memberSelector2)); }
                if (memberSelector3 is null) { throw new ArgumentNullException(nameof(memberSelector3)); }

                Ignores.Add(memberSelector1);
                Ignores.Add(memberSelector2);
                Ignores.Add(memberSelector3);
                return this;
            }

            /// <inheritdoc/>
            ITransformationMap<T> ITransformationMap<T>.Ignore(params string[] memberSelectors)
            {
                if (memberSelectors is null) { throw new ArgumentNullException(nameof(memberSelectors)); }

                Ignores.AddRange(memberSelectors);
                return this;
            }

            #endregion
        }
    }
}
