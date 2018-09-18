// <copyright file="LinkData.Constant.cs" company="Cimpress, Inc.">
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

using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Consting (???) support.</summary>
    public static partial class LinkData
    {
        /// <summary>Represents a link from a constant URI.</summary>
        public sealed class Constant
            : ILinkData
        {
            /// <summary>Initializes a new instance of the <see cref="Constant"/> class.</summary>
            /// <param name="href">The URI that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="href"/> is <see langword="null"/>.</exception>
            public Constant([NotNull] Uri href)
            {
                Href = href ?? throw new ArgumentNullException(nameof(href));
            }

            /// <summary>Initializes a new instance of the <see cref="Constant"/> class.</summary>
            /// <param name="href">The URI that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="href"/> is <see langword="null"/>.</exception>
            /// <exception cref="UriFormatException"><paramref name="href"/> does not represent a valid URI.</exception>
            public Constant([NotNull] string href)
            {
                if (href is null) { throw new ArgumentNullException(nameof(href)); }

                Href = new Uri(href);
            }

            /// <summary>
            /// Gets the location of the target resource or a template which, when bound,
            /// will be the location of the location of the target resource.
            /// </summary>
            [NotNull]
            public Uri Href { get; }

            /// <summary>
            /// Gets or sets a hint to indicate the media type expected
            /// when dereferencing the target resource.
            /// </summary>
            [SuppressMessage("Microsoft:Guidelines", "CA1721", Justification = "That's what it's called.")]
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets whether the link has been deprecated –
            /// that is, whether it will be removed at a future date.
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
        }
    }
}
