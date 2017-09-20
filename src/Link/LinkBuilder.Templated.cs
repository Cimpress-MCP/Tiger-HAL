// <copyright file="LinkBuilder.Templated.cs" company="Cimpress, Inc.">
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
using Tavis.UriTemplates;

namespace Tiger.Hal
{
    /// <content>Templating support.</content>
    public partial class LinkBuilder
    {
        /// <summary>Represents a templated link from a URI template.</summary>
        public sealed class Templated
            : LinkBuilder
        {
            readonly UriTemplate _template;

            /// <summary>Initializes a new instance of the <see cref="LinkBuilder.Templated"/> class.</summary>
            /// <param name="template">The template that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="template"/> is <see langword="null"/>.</exception>
            public Templated([NotNull] UriTemplate template)
            {
                _template = template ?? throw new ArgumentNullException(nameof(template));
            }

            /// <summary>Initializes a new instance of the <see cref="LinkBuilder.Templated"/> class.</summary>
            /// <param name="template">The template that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="template"/> is <see langword="null"/>.</exception>
            public Templated([NotNull] string template)
            {
                if (template == null) { throw new ArgumentNullException(nameof(template)); }

                _template = new UriTemplate(template, resolvePartially: true);
            }

            /// <inheritdoc/>
            internal override Link Build(IUrlHelper urlHelper)
            {
                if (urlHelper == null) { throw new ArgumentNullException(nameof(urlHelper)); }

                return new Link(_template.Resolve(), true, Type, Deprecation, Name, Profile, Title, HrefLang);
            }
        }
    }
}
