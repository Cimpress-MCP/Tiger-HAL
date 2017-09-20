// <copyright file="LinkBuilder.cs" company="Cimpress, Inc.">
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
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Tiger.Hal
{
    /// <summary>Represents a builder for <see cref="Link"/>.</summary>
    [PublicAPI]
    public abstract partial class LinkBuilder
    {
        LinkBuilder()
        {
        }

        /// <summary>
        /// Gets or sets a hint to indicate the media type expected
        /// when dereferencing the target resource.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets whether the link has been deprecated – that is, whether
        /// it will be removed at a future date.
        /// </summary>
        public Uri Deprecation { get; set; }

        /// <summary>
        /// Gets or sets a secondary key for selecting links which
        /// share the same relation type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>Gets or sets a hint about the profile of the target resource.</summary>
        public Uri Profile { get; set; }

        /// <summary>Gets or sets a human-readable identifier for the link.</summary>
        public string Title { get; set; }

        /// <summary>Gets or sets the language of the target resource.</summary>
        public string HrefLang { get; set; }

        /// <summary>Builds a <see cref="Link"/> from this instance.</summary>
        /// <param name="urlHelper">The application's URL generator.</param>
        /// <returns>A <see cref="Link"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="urlHelper"/> is <see langword="null"/>.</exception>
        [NotNull]
        internal abstract Link Build([NotNull] IUrlHelper urlHelper);
    }
}
