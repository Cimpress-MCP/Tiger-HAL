// <copyright file="LinkCollection.Converter.cs" company="Cimpress, Inc.">
//   Copyright 2017 Cimpress, Inc.
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
                        writer.WriteStartArray();
                        foreach (var link in linkCollection) { serializer.Serialize(writer, link); }
                        writer.WriteEndArray();
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
