// <copyright file="LinkData.cs" company="Cimpress, Inc.">
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
using Tavis.UriTemplates;

namespace Tiger.Hal
{
    /// <summary>Convenience methods for creating <see cref="ILinkData"/> instances.</summary>
    public static partial class LinkData
    {
        /// <summary>Creates a link from an endpoint.</summary>
        /// <param name="endpointName">The name of the endpoint for which to generate a link.</param>
        /// <param name="values">The values to use when generating a link.</param>
        /// <returns>A link builder.</returns>
        public static ILinkData Endpoint(string endpointName, object? values = null) =>
            new Endpointed(endpointName, values);

        /// <summary>Creates a templated link from a URI template.</summary>
        /// <param name="template">The template that will become the value of <see cref="Link.Href"/>.</param>
        /// <returns>A link builder.</returns>
        public static ILinkData Template(UriTemplate template) => new Templated(template);

        /// <summary>Creates a templated link from a URI template.</summary>
        /// <param name="template">The template that will become the value of <see cref="Link.Href"/>.</param>
        /// <returns>A link builder.</returns>
        public static ILinkData Template(string template) => new Templated(template);

        /// <summary>Creates a link from a constant URI.</summary>
        /// <param name="href">The URI that will become the value of <see cref="Link.Href"/>.</param>
        /// <returns>A link builder.</returns>
        public static ILinkData Const(Uri href) => new Constant(href);

        /// <summary>Creates a link from a constant URI.</summary>
        /// <param name="href">The URI that will become the value of <see cref="Link.Href"/>.</param>
        /// <returns>A link builder.</returns>
        /// <exception cref="UriFormatException"><paramref name="href"/> does not represent a valid URI.</exception>
        public static ILinkData Const(string href) => new Constant(href);
    }
}
