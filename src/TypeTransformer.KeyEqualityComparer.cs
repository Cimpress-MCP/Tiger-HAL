// <copyright file="TypeTransformer.KeyEqualityComparer.cs" company="Cimpress, Inc.">
//   Copyright 2017 Cimpress, Inc.
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

namespace Tiger.Hal
{
    /// <content>Compares the keys of type transformation.</content>
    sealed partial class TypeTransformer
    {
        /// <inheritdoc/>
        sealed class KeyEqualityComparer
            : IEqualityComparer<(string rel, bool isSingular)>
        {
            readonly StringComparison _comparison;

            /// <summary>Initializes a new instance of the <see cref="KeyEqualityComparer"/> class.</summary>
            /// <param name="comparison">The type of string comparison to use for "rel".</param>
            public KeyEqualityComparer(StringComparison comparison)
            {
                _comparison = comparison;
            }

            /// <inheritdoc/>
            bool IEqualityComparer<(string rel, bool isSingular)>.Equals(
                (string rel, bool isSingular) x,
                (string rel, bool isSingular) y) => string.Equals(x.rel, y.rel, _comparison) &&
                                                    x.isSingular == y.isSingular;

            /// <inheritdoc/>
            int IEqualityComparer<(string rel, bool isSingular)>.GetHashCode((string rel, bool isSingular) obj) =>
                obj.GetHashCode();
        }
    }
}
