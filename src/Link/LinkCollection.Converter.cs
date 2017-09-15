using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tiger.Hal
{
    /// <content>Serialization control.</content>
    public sealed partial class LinkCollection
    {
        /// <summary>Controls JSON serialization of the <see cref="LinkCollection"/> class.</summary>
        internal sealed class Converter
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
