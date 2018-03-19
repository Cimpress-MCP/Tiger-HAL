// <copyright file="LinkData.Templated.cs" company="Cimpress, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Tavis.UriTemplates;

namespace Tiger.Hal
{
    /// <summary>Templating support.</summary>
    public static partial class LinkData
    {
        /// <summary>Represents a templated link from a URI template.</summary>
        public sealed class Templated
            : ILinkData
        {
            /// <summary>Initializes a new instance of the <see cref="Templated"/> class.</summary>
            /// <param name="template">The template that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="template"/> is <see langword="null"/>.</exception>
            public Templated([NotNull] UriTemplate template)
            {
                Template = template ?? throw new ArgumentNullException(nameof(template));
            }

            /// <summary>Initializes a new instance of the <see cref="Templated"/> class.</summary>
            /// <param name="template">The template that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="template"/> is <see langword="null"/>.</exception>
            public Templated([NotNull] string template)
            {
                if (template == null) { throw new ArgumentNullException(nameof(template)); }

                Template = new UriTemplate(template, resolvePartially: true);
            }

            /// <summary>
            /// Gets the template that will become the value of <see cref="Link.Href"/>.
            /// </summary>
            [NotNull]
            public UriTemplate Template { get; }

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
