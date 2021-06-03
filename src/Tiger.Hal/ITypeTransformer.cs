// <copyright file="ITypeTransformer.cs" company="Cimpress, Inc.">
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

using System.Collections.Generic;

namespace Tiger.Hal
{
    /// <summary>Transforms a value into its HAL representation.</summary>
    public interface ITypeTransformer
    {
        /// <summary>Gets a mapping of accessor to embeddable value.</summary>
        IReadOnlyCollection<IEmbedInstruction> Embeds { get; }

        /// <summary>Gets a mapping of accessor to hoistable value.</summary>
        IReadOnlyCollection<IHoistInstruction> Hoists { get; }

        /// <summary>Gets a collection of ignorable properties.</summary>
        IReadOnlyCollection<string> Ignores { get; }

        /// <summary>
        /// Generates a mapping of link relations to a collection of links
        /// for the given value.
        /// </summary>
        /// <param name="value">The value for which to generate link relations.</param>
        /// <returns>A mapping of link relations to link collection.</returns>
        /// <remarks>
        /// Remember that <see cref="LinkCollection"/> serializes to a single object
        /// if the collection is singular.
        /// </remarks>
        IReadOnlyDictionary<string, LinkCollection> GenerateLinks(object value);
    }
}
