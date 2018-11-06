// <copyright file="ItemsEmbedInstruction.cs" company="Cimpress, Inc.">
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
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for embedding a collection in a HAL response.</summary>
    abstract class ItemsEmbedInstruction
        : IEmbedInstruction
    {
        /// <summary>The index that represents embedding one's elements.</summary>
        public const string ElementsIndex = "[*]";

        /// <summary>Initializes a new instance of the <see cref="ItemsEmbedInstruction"/> class.</summary>
        /// <param name="relation">The relation under which to embed the elements.</param>
        protected ItemsEmbedInstruction(string relation)
        {
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }

        /// <inheritdoc/>
        public string Relation { get; }

        /// <inheritdoc/>
        public object Index => ElementsIndex;

        /// <inheritdoc/>
        public abstract Type Type { get; }

        /// <inheritdoc/>
        public abstract object GetEmbedValue([NotNull] object main);
    }
}
