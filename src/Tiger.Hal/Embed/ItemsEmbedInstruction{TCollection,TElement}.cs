// <copyright file="ItemsEmbedInstruction{TCollection,TElement}.cs" company="Cimpress, Inc.">
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
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for embedding a collection in a HAL response.</summary>
    /// <typeparam name="TCollection">The collection type to embed.</typeparam>
    /// <typeparam name="TElement">The element type of <typeparamref name="TCollection"/>.</typeparam>
    sealed class ItemsEmbedInstruction<TCollection, TElement>
        : ItemsEmbedInstruction
        where TCollection : IReadOnlyCollection<TElement>
    {
        /// <summary>Initializes a new instance of the <see cref="ItemsEmbedInstruction{TCollection, TElement}"/> class.</summary>
        /// <param name="relation">The relation under which to embed the elements.</param>
        public ItemsEmbedInstruction(string relation)
            : base(relation)
        {
        }

        /// <inheritdoc/>
        public override Type Type => typeof(IReadOnlyCollection<TElement>);

        /// <inheritdoc/>
        public override object GetEmbedValue([NotNull] object main) => (TCollection)main;
    }
}
