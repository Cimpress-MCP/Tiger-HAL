using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tiger.Hal
{
    /// <summary>Represents a collection of links, with possible special handling for collections of one.</summary>
    [JsonArray, JsonConverter(typeof(Converter))]
    public sealed class LinkCollection
        : ReadOnlyCollection<Link>
    {
        readonly bool _isSingular;

        /// <summary>Initializes a new instance of the <see cref="LinkCollection"/> class.</summary>
        /// <param name="links">The collection of links.</param>
        /// <param name="isSingular">Whether this collection should be serialized as a collection unconditionally.</param>
        /// <exception cref="ArgumentNullException"><paramref name="links" /> is <see langword="null"/>.</exception>
        public LinkCollection([NotNull] IList<Link> links, bool isSingular = false)
            : base(links)
        {
            _isSingular = isSingular;
        }

        sealed class Converter
            : JsonConverter
        {
            /// <inheritdoc/>
            public override bool CanRead { get; } = false;

            /// <inheritdoc/>
            public override void WriteJson(
                JsonWriter writer,
                [CanBeNull] object value,
                [NotNull] JsonSerializer serializer)
            {
                if (value == null)
                { // note(cosborn) Frankly, something has gone wrong.
                    serializer.Serialize(writer, null);
                    return;
                }

                var linkCollection = (LinkCollection)value;
                switch (linkCollection.Count)
                {
                    case 0:
                        return;
                    case 1:
                        if (!linkCollection._isSingular) { goto default; }
                        serializer.Serialize(writer, linkCollection.Single(), typeof(Link));
                        return;
                    default:
                        serializer.Serialize(writer, linkCollection.ToList(), typeof(List<Link>));
                        return;
                }
            }

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException"><see cref="CanRead"/> is <see langword="false"/>.</exception>
            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer) =>
                throw new NotSupportedException("CanRead is false.");

            /// <inheritdoc/>
            public override bool CanConvert([NotNull] Type objectType) =>
                objectType == typeof(LinkCollection);
        }
    }
}
