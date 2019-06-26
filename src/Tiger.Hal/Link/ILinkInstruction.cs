// <copyright file="ILinkInstruction.cs" company="Cimpress, Inc.">
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
    /// <summary>Represents an instruction for creating a link relation in a HAL response.</summary>
    public interface ILinkInstruction
    {
        /// <summary>
        /// Gets a value indicating whether this link instruction represents a single value
        /// when it produces a singleton collection from <see cref="TransformToLinkBuilders"/>.
        /// </summary>
        /// <param name="main">The main object from which to determine cardinality.</param>
        /// <returns><see langword="true"/> if <paramref name="main"/> represents a singular object; otherwise, <see langword="false"/>.</returns>
        bool IsSingular([NotNull] object main);

        /// <summary>
        /// Transforms this instance into a collection of instances of <see cref="ILinkData"/>.
        /// </summary>
        /// <param name="main">The main object from which to create a link collection.</param>
        /// <returns>A collection of instances of <see cref="ILinkData"/>.</returns>
        [NotNull, ItemNotNull]
        IEnumerable<ILinkData> TransformToLinkBuilders([NotNull] object main);
    }
}
