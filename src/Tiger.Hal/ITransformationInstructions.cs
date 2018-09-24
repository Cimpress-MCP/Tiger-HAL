// <copyright file="ITransformationInstructions.cs" company="Cimpress, Inc.">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>
    /// Represents the instructions necessary for transforming
    /// a value into its HAL representation.
    /// </summary>
    interface ITransformationInstructions
    {
        /// <summary>
        /// Gets a collection of instructions for creating link relations.
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, ILinkInstruction> LinkInstructions { get; }

        /// <summary>
        /// Gets a collection of instructions for embedding values.
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<IEmbedInstruction> EmbedInstructions { get; }

        /// <summary>
        /// Gets a collection of instructions for hoisting values.
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<IHoistInstruction> HoistInstructions { get; }

        /// <summary>
        /// Gets a collection of instructions for ignoring values.
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyCollection<string> IgnoreInstructions { get; }
    }
}
