// <copyright file="MemberEmbedInstruction{T,TMember}.cs" company="Cimpress, Inc.">
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
using Newtonsoft.Json.Linq;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for embedding a value in a HAL response.</summary>
    /// <typeparam name="T">The parent type of the value to embed.</typeparam>
    /// <typeparam name="TMember">The type of the value to embed.</typeparam>
    sealed class MemberEmbedInstruction<T, TMember>
        : IEmbedInstruction
    {
        readonly Func<T, TMember> _valueSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberEmbedInstruction{T,TSelected}"/> class.
        /// </summary>
        /// <param name="relation">The name of the link relation to establish.</param>
        /// <param name="memberName">The path into the object to select the value to embed.</param>
        /// <param name="valueSelector">A function that selects a value to embed.</param>
        public MemberEmbedInstruction(
            string relation,
            string memberName,
            Func<T, TMember> valueSelector)
        {
            Relation = relation;
            Index = memberName;
            _valueSelector = valueSelector;
        }

        /// <inheritdoc/>
        public string Relation { get; }

        /// <inheritdoc/>
        public string Index { get; }

        /// <inheritdoc/>
        public JToken? GetEmbedValue(object main, Func<object?, Type, JToken?> visitor) =>
            visitor(_valueSelector((T)main), typeof(TMember));
    }
}
