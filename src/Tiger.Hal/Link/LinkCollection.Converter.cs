// <copyright file="LinkCollection.Converter.cs" company="Cimpress, Inc.">
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
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tiger.Hal
{
    /// <summary>Serialization control.</summary>
    public sealed partial class LinkCollection
    {
        /// <summary>Controls JSON serialization of the <see cref="LinkCollection"/> class.</summary>
        internal sealed class Converter
            : JsonConverter<LinkCollection>
        {
            /// <inheritdoc/>
            public override bool CanRead => false;

            /// <inheritdoc/>
            public override void WriteJson(
                JsonWriter writer,
                [CanBeNull] LinkCollection value,
                [NotNull] JsonSerializer serializer)
            {
                if (value is null)
                { // note(cosborn) Frankly, something has gone wrong.
                    serializer.Serialize(writer, null);
                    return;
                }

                switch (value.Count)
                {
                    case 0:
                        return;
                    case 1:
                        if (!value._isSingular) { goto default; }
                        serializer.Serialize(writer, value.Single(), typeof(Link));
                        return;
                    default:
                        writer.WriteStartArray();
                        foreach (var link in value)
                        {
                            serializer.Serialize(writer, link);
                        }

                        writer.WriteEndArray();
                        return;
                }
            }

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException"><see cref="CanRead"/> is <see langword="false"/>.</exception>
            public override LinkCollection ReadJson(
                JsonReader reader,
                Type objectType,
                LinkCollection existingValue,
                bool hasExistingValue,
                JsonSerializer serializer) =>
                throw new NotSupportedException("CanRead is false.");
        }
    }
}
