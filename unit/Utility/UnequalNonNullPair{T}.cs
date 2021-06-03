// <copyright file="UnequalNonNullPair{T}.cs" company="Cimpress, Inc.">
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

namespace Test.Utility
{
    /// <summary>Generates a pair of values which are non-<see langword="null"/> and unequal.</summary>
    /// <typeparam name="T">The member type.</typeparam>
    public struct UnequalNonNullPair<T>
        : IEquatable<UnequalNonNullPair<T>>
        where T : class
    {
        readonly (T Left, T Right) _values;

        /// <summary>Initializes a new instance of the <see cref="UnequalNonNullPair{T}"/> struct.</summary>
        /// <param name="values">The values of the pair.</param>
        public UnequalNonNullPair((T Left, T Right) values)
        {
            _values = values;
        }

        /// <summary>Gets the left value.</summary>
        public T Left => _values.Left;

        /// <summary>Gets the right value.</summary>
        public T Right => _values.Right;

        public static bool operator ==(UnequalNonNullPair<T> left, UnequalNonNullPair<T> right) => left.Equals(right);

        public static bool operator !=(UnequalNonNullPair<T> left, UnequalNonNullPair<T> right) => !(left == right);

        /// <inheritdoc/>
        public override string ToString() => $"UnequalNonNullPair {_values}";

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is UnequalNonNullPair<T> pair && Equals(pair);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Left, Right);

        /// <inheritdoc/>
        public bool Equals(UnequalNonNullPair<T> other) => EqualityComparer<T>.Default.Equals(Left, other.Left)
            && EqualityComparer<T>.Default.Equals(Right, other.Right);
    }
}
