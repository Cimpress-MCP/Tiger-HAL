using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tiger.Hal
{
    /// <summary>Represents a collection of links.</summary>
    [JsonArray, JsonConverter(typeof(UnwrappingLinkCollectionConverter))]
    public sealed class LinkCollection
        : ReadOnlyCollection<Link>
    {
        /// <summary>Initializes a new instance of the <see cref="LinkCollection"/> class.</summary>
        /// <param name="links">The collection of links.</param>
        /// <exception cref="ArgumentNullException"><paramref name="links" /> is <see langword="null"/>.</exception>
        public LinkCollection([NotNull] IList<Link> links)
            : base(links)
        {
        }
    }
}
