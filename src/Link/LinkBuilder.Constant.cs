// <copyright file="LinkBuilder.Constant.cs" company="Cimpress, Inc.">
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
    /// <content>Consting (???) support.</content>
    public partial class LinkBuilder
    {
        /// <summary>Represents a link from a constant URI.</summary>
        public sealed class Constant
            : LinkBuilder
        {
            readonly Uri _href;

            /// <summary>Initializes a new instance of the <see cref="LinkBuilder.Constant"/> class.</summary>
            /// <param name="href">The URI that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="href"/> is <see langword="null"/>.</exception>
            public Constant([NotNull] Uri href)
            {
                _href = href ?? throw new ArgumentNullException(nameof(href));
            }

            /// <summary>Initializes a new instance of the <see cref="LinkBuilder.Constant"/> class.</summary>
            /// <param name="href">The URI that will become the value of <see cref="Link.Href"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="href"/> is <see langword="null"/>.</exception>
            /// <exception cref="UriFormatException"><paramref name="href"/> does not represent a valid URI.</exception>
            public Constant([NotNull] string href)
            {
                if (href == null) { throw new ArgumentNullException(nameof(href)); }

                _href = new Uri(href);
            }

            /// <inheritdoc/>
            internal override Link Build(IUrlHelper urlHelper)
            {
                if (urlHelper == null) { throw new ArgumentNullException(nameof(urlHelper)); }

                var href = _href.IsAbsoluteUri
                    ? _href.AbsoluteUri
                    : _href.OriginalString;

                return new Link(href, false, Type, Deprecation, Name, Profile, Title, HrefLang);
            }
        }
    }
}
