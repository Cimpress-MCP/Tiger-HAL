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
