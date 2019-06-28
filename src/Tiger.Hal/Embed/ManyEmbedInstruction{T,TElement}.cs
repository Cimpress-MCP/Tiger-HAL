// <copyright file="ManyEmbedInstruction{T,TElement}.cs" company="Cimpress, Inc.">
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
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for embedding a value in a HAL response.</summary>
    /// <typeparam name="T">The parent type of the value to embed.</typeparam>
    /// <typeparam name="TElement">The element type of the return type of the value selector.</typeparam>
    sealed class ManyEmbedInstruction<T, TElement>
        : IEmbedInstruction
    {
        readonly Func<T, IReadOnlyCollection<TElement>> _valueSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyEmbedInstruction{T, TElement}"/> class.
        /// </summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberName">The path into the object to select the value to embed.</param>
        /// <param name="valueSelector">A function that selects a value to embed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        public ManyEmbedInstruction(
            [NotNull] string relation,
            [NotNull] string memberName,
            [NotNull] Func<T, IReadOnlyCollection<TElement>> valueSelector)
        {
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
            Index = memberName ?? throw new ArgumentNullException(nameof(memberName));
            _valueSelector = valueSelector ?? throw new ArgumentNullException(nameof(valueSelector));
        }

        /// <inheritdoc/>
        public string Relation { get; }

        /// <inheritdoc/>
        public string Index { get; }

        /// <inheritdoc/>
        public JToken GetEmbedValue(object main, Func<object, Type, JToken> visitor)
        {
            if (main is null)
            {
                return JValue.CreateNull();
            }

            return JArray.FromObject(_valueSelector((T)main).Select(o => visitor(o, typeof(TElement))));
        }
    }
}
