// <copyright file="TransformationMap.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;

namespace Tiger.Hal
{
    /// <summary>Defines a series of transformations for a type to its HAL representation.</summary>
    sealed partial class TransformationMap
        : ITransformationMap
    {
        readonly Dictionary<Type, ITransformationInstructions> _maps = new();

        /// <summary>Gets the mapping of transformation instructions.</summary>
        internal IReadOnlyDictionary<Type, ITransformationInstructions> Maps => _maps;

        /// <inheritdoc/>
        ITransformationMap<T> ITransformationMap.Self<T>(
            Func<T, ILinkData?> self)
        {
            var builder = new Builder<T>(self);
            _maps[typeof(T)] = builder;
            return builder;
        }

        /// <inheritdoc/>
        IElementTransformationMap<TCollection, TElement> ITransformationMap.Self<TCollection, TElement>(
            Func<TCollection, ILinkData?> self)
        {
            var builder = new Builder<TCollection, TElement>(self);
            _maps[typeof(TCollection)] = builder;
            return builder;
        }
    }
}
