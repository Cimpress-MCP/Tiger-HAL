using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Tiger.Hal
{
    /// <content>Routing support.</content>
    public partial class LinkBuilder
    {
        /// <summary>Represents a link from an ASP.NET MVC route.</summary>
        /// <remarks>Did you name the route in your controller?</remarks>
        public sealed class Routed
            : LinkBuilder
        {
            readonly string _routeName;
            readonly object _routeValues;

            /// <summary>Initializes a new instance of the <see cref="LinkBuilder.Routed"/> class.</summary>
            /// <param name="routeName">The name of the route for which to generate a link.</param>
            /// <param name="routeValues">The route values to use when generating a link.</param>
            /// <exception cref="ArgumentNullException"><paramref name="routeName"/> is <see langword="null"/>.</exception>
            public Routed([NotNull] string routeName, [CanBeNull] object routeValues = null)
            {
                _routeName = routeName ?? throw new ArgumentNullException(nameof(routeName));
                _routeValues = routeValues ?? new { };
            }

            /// <inheritdoc/>
            internal override Link Build(IUrlHelper urlHelper)
            {
                if (urlHelper == null) { throw new ArgumentNullException(nameof(urlHelper)); }

                var href = urlHelper.Link(_routeName, _routeValues);
                return new Link(href, false, Type, Deprecation, Name, Profile, Title, HrefLang);
            }
        }
    }
}
