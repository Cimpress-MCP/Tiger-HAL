// <copyright file="AbsoluteUri.cs" company="Cimpress, Inc.">
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

namespace Test.Utility
{
    /// <summary>Represents a <see cref="Uri"/> with, at least, protocol and domain.</summary>
    sealed record AbsoluteUri(Uri Uri)
    {
        /// <summary>Converts an absolute URI to an undistinguished URI.</summary>
        /// <param name="uri">The URI to convert.</param>
        public static implicit operator Uri(AbsoluteUri uri) => uri.Uri;

        /// <summary>Converts an absolute URI to an undistinguished URI.</summary>
        /// <returns>The underlying instance of <see cref="Uri"/>.</returns>
        public Uri ToUri() => Uri;
    }
}
