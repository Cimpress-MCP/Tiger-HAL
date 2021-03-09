// <copyright file="LinkBuilder.cs" company="Cimpress, Inc.">
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using static System.Globalization.CultureInfo;

namespace Tiger.Hal
{
    /// <summary>Transformations from the built-in implementations of <see cref="ILinkData"/>.</summary>
    static class LinkBuilder
    {
        /// <summary>
        /// Transforms an instance of <see cref="LinkData.Constant"/> to an instance of <see cref="Link"/>.
        /// </summary>
        public sealed class Constant
            : ILinkBuilder<LinkData.Constant>
        {
            /// <inheritdoc/>
            Link ILinkBuilder<LinkData.Constant>.Build(LinkData.Constant linkData)
            {
                var href = linkData.Href.IsAbsoluteUri
                    ? linkData.Href.AbsoluteUri
                    : linkData.Href.OriginalString;

                return new Link(
                    href,
                    false,
                    linkData.Type,
                    linkData.Deprecation,
                    linkData.Name,
                    linkData.Profile,
                    linkData.Title,
                    linkData.HrefLang);
            }
        }

        /// <summary>
        /// Transforms an instance of <see cref="LinkData.Templated"/> to an instance of <see cref="Link"/>.
        /// </summary>
        public sealed class Templated
            : ILinkBuilder<LinkData.Templated>
        {
            /// <inheritdoc/>
            Link ILinkBuilder<LinkData.Templated>.Build(LinkData.Templated linkData) => new(
                linkData.Template.Resolve(),
                true,
                linkData.Type,
                linkData.Deprecation,
                linkData.Name,
                linkData.Profile,
                linkData.Title,
                linkData.HrefLang);
        }

        /// <summary>
        /// Transforms an instance of <see cref="LinkData.Routed"/> to an instance of <see cref="Link"/>.
        /// </summary>
        public sealed class Routed
            : ILinkBuilder<LinkData.Routed>
        {
            readonly IActionContextAccessor _actionContextAccessor;
            readonly IUrlHelperFactory _urlHelperFactory;

            /// <summary>Initializes a new instance of the <see cref="Routed"/> class.</summary>
            /// <param name="actionContextAccessor">The application's <see cref="ActionContext"/> accessor.</param>
            /// <param name="urlHelperFactory">The application's <see cref="IUrlHelper"/> factory.</param>
            /// <exception cref="ArgumentNullException"><paramref name="actionContextAccessor"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="urlHelperFactory"/> is <see langword="null"/>.</exception>
            public Routed(
                IActionContextAccessor actionContextAccessor,
                IUrlHelperFactory urlHelperFactory)
            {
                _actionContextAccessor = actionContextAccessor;
                _urlHelperFactory = urlHelperFactory;
            }

            /// <inheritdoc/>
            Link ILinkBuilder<LinkData.Routed>.Build(LinkData.Routed linkData)
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                var href = urlHelper.Link(linkData.RouteName, linkData.RouteValues);
                return href is null
                    ? throw new InvalidOperationException(string.Format(InvariantCulture, @"No route exists with name ""{0}"".", linkData.RouteName))
                    : new(href, false, linkData.Type, linkData.Deprecation, linkData.Name, linkData.Profile, linkData.Title, linkData.HrefLang);
            }
        }
    }
}
