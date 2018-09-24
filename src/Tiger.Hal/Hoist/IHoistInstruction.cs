// <copyright file="IHoistInstruction.cs" company="Cimpress, Inc.">
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

using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for creating a hoist in a HAL response.</summary>
    public interface IHoistInstruction
    {
        /// <summary>Gets the path into the object to select the value to hoist.</summary>
        [NotNull]
        string Name { get; }

        /// <summary>Retrieves the value to hoist from the given main object.</summary>
        /// <param name="main">The main object.</param>
        /// <returns>The value to hoist.</returns>
        [NotNull]
        object GetHoistValue([NotNull] object main);
    }
}
