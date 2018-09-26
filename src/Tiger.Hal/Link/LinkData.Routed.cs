// <copyright file="LinkData.Routed.cs" company="Cimpress, Inc.">
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
    /// <summary>Routing support.</summary>
    public static partial class LinkData
    {
        /// <summary>Represents a link from an ASP.NET MVC route.</summary>
        /// <remarks>Be sure that you named the route in your controller.</remarks>
        public sealed class Routed
            : ILinkData
        {
            /// <summary>Initializes a new instance of the <see cref="LinkData.Routed"/> class.</summary>
            /// <param name="routeName">The name of the route for which to generate a link.</param>
            /// <param name="routeValues">The route values to use when generating a link.</param>
            /// <exception cref="ArgumentNullException"><paramref name="routeName"/> is <see langword="null"/>.</exception>
            public Routed([NotNull] string routeName, [CanBeNull] object routeValues = null)
            {
                RouteName = routeName ?? throw new ArgumentNullException(nameof(routeName));
                RouteValues = routeValues ?? new { };
            }

            /// <summary>Gets the name of the route for which to generate a link.</summary>
            [NotNull]
            public string RouteName { get; }

            /// <summary>Gets the values required to generate a link.</summary>
            [NotNull]
            public object RouteValues { get; }

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
