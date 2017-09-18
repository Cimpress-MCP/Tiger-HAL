using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents a collection of links, with possible special handling for collections of one.</summary>
    public sealed partial class LinkCollection
        : ReadOnlyCollection<Link>
    {
        readonly bool _isSingular;

        /// <summary>Initializes a new instance of the <see cref="LinkCollection"/> class.</summary>
        /// <param name="links">The collection of links.</param>
        /// <param name="isSingular">Whether this collection should be serialized as a collection unconditionally.</param>
        /// <exception cref="ArgumentNullException"><paramref name="links" /> is <see langword="null"/>.</exception>
        public LinkCollection([NotNull] IList<Link> links, bool isSingular)
            : base(links)
        {
            _isSingular = isSingular;
        }
    }
}
