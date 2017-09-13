using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tiger.Hal
{
    /// <summary>Converts a collection of links into JSON, unwrapping singular collections.</summary>
    sealed class UnwrappingLinkCollectionConverter
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
            if (linkCollection.Count == 1)
            {
                serializer.Serialize(writer, linkCollection.Single(), typeof(Link));
            }
            else
            {
                serializer.Serialize(writer, linkCollection, typeof(LinkCollection));
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
