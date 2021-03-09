// <copyright file="LinkEqualityComparer.cs" company="Cimpress, Inc.">
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
using Tiger.Hal;
using static System.StringComparison;

namespace Test.Utility
{
    /// <summary>Compares two instances of <see cref="Link"/> for equality.</summary>
    sealed class LinkEqualityComparer
        : EqualityComparer<Link>
    {
        /// <inheritdoc/>
        public override bool Equals(Link? x, Link? y) => ReferenceEquals(x, y)
            || (x is not null && y is not null
            && string.Equals(x.Href, y.Href, Ordinal)
            && x.IsTemplated == y.IsTemplated
            && string.Equals(x.Type, y.Type, Ordinal)
            && x.Deprecation == y.Deprecation
            && string.Equals(x.Name, y.Name, Ordinal)
            && x.Profile == y.Profile
            && string.Equals(x.Title, y.Title, Ordinal)
            && string.Equals(x.HrefLang, y.HrefLang, OrdinalIgnoreCase));

        /// <inheritdoc/>
        public override int GetHashCode(Link? obj) =>
            HashCode.Combine(obj?.Href, obj?.IsTemplated, obj?.Type, obj?.Name, obj?.Profile, obj?.Title, obj?.HrefLang);
    }
}
