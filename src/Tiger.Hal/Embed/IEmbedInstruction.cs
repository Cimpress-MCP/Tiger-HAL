// <copyright file="IEmbedInstruction.cs" company="Cimpress, Inc.">
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
    /* note(cosborn)
     * Embed Instructions are complex to support the scenario
     * of embedding properties on an object that serializes
     * to an array. Json.NET won't let me get a valid
     * JsonObjectContract for a thing that "should" have a
     * JsonArrayContract, so we have to introduce a little
     * of the ol' razzle-dazzle.
     */

    /// <summary>Represents an instruction for creating an embed in a HAL response.</summary>
    public interface IEmbedInstruction
    {
        /// <summary>Gets the name of the link relation to establish.</summary>
        [NotNull]
        string Relation { get; }

        /// <summary>Gets the path into the object to select the value to embed.</summary>
        [NotNull]
        object Index { get; }

        /// <summary>Gets the type of the value to embed.</summary>
        [NotNull]
        Type Type { get; }

        /// <summary>Retrieves the value to embed from the given main object.</summary>
        /// <param name="main">The main object.</param>
        /// <returns>The value to embed.</returns>
        [NotNull]
        object GetEmbedValue([NotNull] object main);
    }
}
