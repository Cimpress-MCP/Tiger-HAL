// <copyright file="IHalProfile.cs" company="Cimpress, Inc.">
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
    /// <summary>Defines the HAL+JSON transformations for an application.</summary>
    [PublicAPI]
    public interface IHalProfile
    {
        /// <summary>Configures the transformation from a type to its HAL+JSON representation.</summary>
        /// <param name="transformationMap">The application's map of HAL transformation.</param>
        void OnTransformationMapCreating([NotNull] ITransformationMap transformationMap);
    }
}
