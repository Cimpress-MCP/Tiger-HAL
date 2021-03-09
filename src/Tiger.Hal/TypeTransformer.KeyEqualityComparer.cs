// <copyright file="TypeTransformer.KeyEqualityComparer.cs" company="Cimpress, Inc.">
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
    /// <summary>Compares the keys of type transformation.</summary>
    sealed partial class TypeTransformer
    {
        /// <inheritdoc/>
        sealed class KeyEqualityComparer
            : IEqualityComparer<(string Rel, bool IsSingular)>
        {
            readonly StringComparer _comparer;

            /// <summary>Initializes a new instance of the <see cref="KeyEqualityComparer"/> class.</summary>
            /// <param name="comparer">The string comparer to use for "rel".</param>
            public KeyEqualityComparer(StringComparer comparer)
            {
                _comparer = comparer;
            }

            /// <inheritdoc/>
            bool IEqualityComparer<(string Rel, bool IsSingular)>.Equals(
                (string Rel, bool IsSingular) x,
                (string Rel, bool IsSingular) y) =>
                    _comparer.Equals(x.Rel, y.Rel) && x.IsSingular == y.IsSingular;

            /// <inheritdoc/>
            int IEqualityComparer<(string Rel, bool IsSingular)>.GetHashCode((string Rel, bool IsSingular) obj)
            {
                var hash = default(HashCode);
                hash.Add(obj.Rel, _comparer);
                hash.Add(obj.IsSingular);
                return hash.ToHashCode();
            }
        }
    }
}
