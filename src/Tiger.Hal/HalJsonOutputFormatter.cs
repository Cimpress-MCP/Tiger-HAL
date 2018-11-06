// <copyright file="HalJsonOutputFormatter.cs" company="Cimpress, Inc.">
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
using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static System.Reflection.BindingFlags;
using static Tiger.Hal.ItemsEmbedInstruction;

namespace Tiger.Hal
{
    /// <summary>A <see cref="JsonOutputFormatter"/> for HAL+JSON content.</summary>
    [PublicAPI]
    public sealed class HalJsonOutputFormatter
        : JsonOutputFormatter
    {
        const string LinksKey = "_links";
        const string EmbeddedKey = "_embedded";

        readonly IHalRepository _halRepository;

        // note(cosborn) Cache the reflection; it's relatively expensive.
        readonly MethodInfo _cocInfo = typeof(DefaultContractResolver)
            .GetMethod("CreateObjectContract", Instance | NonPublic);

        /// <summary>Initializes a new instance of the <see cref="HalJsonOutputFormatter"/> class.</summary>
        /// <param name="serializerSettings">
        /// The <see cref="JsonSerializerSettings"/>. Should be either the application-wide settings
        /// (<see cref="MvcJsonOptions.SerializerSettings"/>) or an instance
        /// <see cref="JsonSerializerSettingsProvider.CreateSerializerSettings"/> initially returned.
        /// </param>
        /// <param name="charPool">The <see cref="ArrayPool{T}"/>.</param>
        /// <param name="halRepository">The application's HAL+JSON repository.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serializerSettings"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="charPool"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="halRepository"/> is <see langword="null"/>.</exception>
        public HalJsonOutputFormatter(
            [NotNull] JsonSerializerSettings serializerSettings,
            [NotNull] ArrayPool<char> charPool,
            [NotNull] IHalRepository halRepository)
            : base(serializerSettings, charPool)
        {
            _halRepository = halRepository ?? throw new ArgumentNullException(nameof(halRepository));

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add("application/hal+json");
        }

        /// <inheritdoc/>
        public override Task WriteResponseBodyAsync(
            [NotNull] OutputFormatterWriteContext context,
            Encoding selectedEncoding)
        {
            if (context.Object is null)
            { // note(cosborn) What else are we supposed to do here, huh?
                return base.WriteResponseBodyAsync(context, selectedEncoding);
            }

            var halValue = Visit(context.Object, context.ObjectType);

            var newContext = new OutputFormatterWriteContext(
                context.HttpContext,
                context.WriterFactory,
                typeof(JToken),
                halValue);

            return base.WriteResponseBodyAsync(newContext, selectedEncoding);
        }

        /// <inheritdoc/>
        protected override bool CanWriteType([NotNull] Type type) => _halRepository.CanTransform(type);

        [CanBeNull]
        JToken Visit([NotNull] object value, [NotNull] Type type)
        {
            switch (SerializerSettings.ContractResolver.ResolveContract(type))
            {
                case JsonObjectContract joc:
                    return VisitObject(joc, JObject.FromObject(value, CreateJsonSerializer()), value);
                case JsonArrayContract jac:
                    return VisitArray(jac, JArray.FromObject(value, CreateJsonSerializer()), (IEnumerable)value);
                case JsonDictionaryContract jdc: // note(cosborn) Don't support defining these yet, but sure why not
                    return VisitDictionary(jdc, JObject.FromObject(value, CreateJsonSerializer()), (IDictionary)value);
                default: // todo(cosborn) Dynamic? Something else?
                    return JToken.FromObject(value, CreateJsonSerializer());
            }
        }

        [CanBeNull]
        JToken VisitObject(
            [NotNull] JsonObjectContract jsonObjectContract,
            [CanBeNull] JObject jObject,
            [CanBeNull] object value)
        {
            if (value is null || jObject is null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonObjectContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jObject;
            }

            var visitQuads =
                from property in jsonObjectContract.Properties
                let jPropertyValue = jObject[property.PropertyName]
                where jPropertyValue != null
                let nativeValue = property.ValueProvider.GetValue(value)
                let propertyValueContract = SerializerSettings.ContractResolver.ResolveContract(property.PropertyType)
                select (name: property.PropertyName, jPropertyValue, nativeValue, propertyValueContract);
            foreach (var (name, jPropertyValue, nativeValue, propertyValueContract) in visitQuads)
            {
                switch ((propertyValueContract, jPropertyValue.Type))
                {
                    case var tuple when tuple.propertyValueContract is JsonObjectContract joc && tuple.Type == JTokenType.Object:
                        jObject[name] = VisitObject(joc, (JObject)jPropertyValue, nativeValue);
                        break;
                    case var tuple when tuple.propertyValueContract is JsonArrayContract jac && tuple.Type == JTokenType.Array:
                        jObject[name] = VisitArray(jac, (JArray)jPropertyValue, (IEnumerable)nativeValue);
                        break;
                    case var tuple when tuple.propertyValueContract is JsonDictionaryContract jdc && tuple.Type == JTokenType.Object:
                        jObject[name] = VisitDictionary(jdc, (JObject)jPropertyValue, (IDictionary)nativeValue);
                        break;

                    // todo(cosborn) Dynamic? Something else?
                }
            }

            var links = transformer.GenerateLinks(value);
            var populatedLinks = links.Where(kvp => kvp.Value.Count != 0).ToImmutableDictionary();
            if (populatedLinks.Count != 0)
            {
                jObject[LinksKey] = JObject.FromObject(populatedLinks, CreateJsonSerializer());
            }

            var embeds = ImmutableList<JProperty>.Empty;
            var embedPairs =
                from embedInstruction in transformer.Embeds
                let embedIndex = embedInstruction.Index.ToString()
                join property in jsonObjectContract.Properties
                    on embedIndex equals property.UnderlyingName
                let embedValue = embedInstruction.GetEmbedValue(value)
                let jEmbedValue = embedValue is null
                    ? JValue.CreateNull()
                    : Visit(embedValue, embedInstruction.Type)
                let jProperty = new JProperty(embedInstruction.Relation, jEmbedValue)
                select (index: property.PropertyName, jProperty);
            foreach (var (index, jProperty) in embedPairs)
            {
                embeds = embeds.Add(jProperty);
                /* note(cosborn)
                 * Remember, indexing the JToken will give us a value within
                 * a JProperty. We can't remove the value from the property
                 * (what would that mean?), so we move up one level to remove
                 * the entire property from the parent object.
                 */
                jObject[index]?.Parent.Remove();
            }

            if (embeds.Count != 0)
            {
                jObject[EmbeddedKey] = new JObject(embeds);
            }

            var ignores =
                from i in transformer.Ignores
                join p in jsonObjectContract.Properties
                    on i equals p.UnderlyingName
                select p.PropertyName;
            foreach (var ignore in ignores)
            {
                jObject[ignore]?.Parent.Remove();
            }

            return jObject;
        }

        [CanBeNull]
        JToken VisitArray(
            [NotNull] JsonArrayContract jsonArrayContract,
            [CanBeNull] JArray jArray,
            [CanBeNull] IEnumerable value)
        {
            if (value is null || jArray is null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonArrayContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jArray;
            }

            var collectionItemContract = SerializerSettings.ContractResolver.ResolveContract(jsonArrayContract.CollectionItemType);
            var visitPairs =
                from indexPair in value.Cast<object>().Select((o, i) => (index: i, nativeValue: o))
                let jIndexValue = jArray[indexPair.index]
                select (indexPair, jIndexValue);
            foreach (var ((index, nativeValue), jIndexValue) in visitPairs)
            {
                switch ((collectionItemContract, jIndexValue.Type))
                {
                    case var tuple when tuple.collectionItemContract is JsonObjectContract joc && tuple.Type == JTokenType.Object:
                        jArray[index] = VisitObject(joc, (JObject)jIndexValue, nativeValue);
                        break;
                    case var tuple when tuple.collectionItemContract is JsonArrayContract jac && tuple.Type == JTokenType.Array:
                        jArray[index] = VisitArray(jac, (JArray)jIndexValue, (IEnumerable)nativeValue);
                        break;
                    case var tuple when tuple.collectionItemContract is JsonDictionaryContract jdc && tuple.Type == JTokenType.Object:
                        jArray[index] = VisitDictionary(jdc, (JObject)jIndexValue, (IDictionary)nativeValue);
                        break;

                    // todo(cosborn) Dynamic? Something else?
                }
            }

            // note(cosborn) Lists embed themselves in a wrapper object.
            var wrapperObject = new JObject();

            var links = transformer.GenerateLinks(value);
            var populatedLinks = links.Where(kvp => kvp.Value.Count != 0).ToImmutableDictionary();
            if (populatedLinks.Count != 0)
            {
                wrapperObject[LinksKey] = JObject.FromObject(populatedLinks, CreateJsonSerializer());
            }

            var embeds = ImmutableArray<JProperty>.Empty;
            var embedPairs =
                from embedInstruction in transformer.Embeds
                where !(embedInstruction.Index is string index && index == ElementsIndex)
                let embedValue = embedInstruction.GetEmbedValue(value)
                let jEmbedValue = embedValue is null
                    ? JValue.CreateNull()
                    : Visit(embedValue, embedInstruction.Type)
                let jProperty = new JProperty(embedInstruction.Relation, jEmbedValue)
                select (index: embedInstruction.Index, jProperty);
            foreach (var (index, jProperty) in embedPairs)
            {
                embeds = embeds.Add(jProperty);
                if (index is int arrayIndex)
                { // note(cosborn) Json.NET will panic if we send in anything but an int.
                    jArray[arrayIndex]?.Remove();
                }
            }

            var embedItems = transformer.Embeds.SingleOrDefault(i => i.Index is string index && index == ElementsIndex);
            if (embedItems != null)
            {
                embeds = embeds.Add(new JProperty(embedItems.Relation, jArray));
            }

            if (embeds.Length != 0)
            {
                wrapperObject[EmbeddedKey] = new JObject(embeds);
            }

            if (transformer.Hoists.Count != 0)
            { // todo(cosborn) Check count because object contract creation is relatively expensive due to reflection.
                var objectContract = CreateObjectContract(jsonArrayContract.UnderlyingType);

                var pairs =
                    from hoist in transformer.Hoists
                    join property in objectContract.Properties
                        on hoist.Name equals property.UnderlyingName
                    let hoistValue = hoist.GetHoistValue(value)
                    let jTokenValue = JToken.FromObject(hoistValue, CreateJsonSerializer())
                    select (name: property.PropertyName, jTokenValue);
                foreach (var (name, jTokenValue) in pairs)
                {
                    wrapperObject[name] = jTokenValue;
                }
            }

            return wrapperObject;
        }

        [CanBeNull]
        JToken VisitDictionary(
            [NotNull] JsonDictionaryContract jsonDictionaryContract,
            [CanBeNull] JObject jObject,
            [CanBeNull] IDictionary value)
        {
            if (value is null || jObject is null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonDictionaryContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jObject;
            }

            var dictionaryValueContract = SerializerSettings.ContractResolver.ResolveContract(jsonDictionaryContract.DictionaryValueType);
            foreach (var key in value.Keys)
            {
                var jKey = jsonDictionaryContract.DictionaryKeyResolver(key.ToString());
                var jValue = jObject[jKey];
                switch ((dictionaryValueContract, jValue.Type))
                {
                    case var tuple when tuple.dictionaryValueContract is JsonObjectContract joc && tuple.Type == JTokenType.Object:
                        jObject[jKey] = new JProperty(jKey, VisitObject(joc, (JObject)jObject[jKey], value[key]));
                        break;
                    case var tuple when tuple.dictionaryValueContract is JsonArrayContract jac && tuple.Type == JTokenType.Array:
                        jObject[jKey] = new JProperty(jKey, VisitArray(jac, (JArray)jObject[jKey], (IEnumerable)value[key]));
                        break;
                    case var tuple when tuple.dictionaryValueContract is JsonDictionaryContract jdc && tuple.Type == JTokenType.Object:
                        jObject[jKey] = new JProperty(jKey, VisitDictionary(jdc, (JObject)jObject[jKey], (IDictionary)value[key]));
                        break;

                    // todo(cosborn) Dynamic? Something else?
                }
            }

            var links = transformer.GenerateLinks(value);
            var populatedLinks = links.Where(kvp => kvp.Value.Count != 0).ToImmutableDictionary();
            if (populatedLinks.Count != 0)
            {
                jObject[LinksKey] = JObject.FromObject(populatedLinks, CreateJsonSerializer());
            }

            var embeds = ImmutableList<JProperty>.Empty;
            var embedPairs =
                from embedInstruction in transformer.Embeds
                let nativeKey = embedInstruction.Index.ToString()
                let embedKey = jsonDictionaryContract.DictionaryKeyResolver(nativeKey)
                let index = embedKey ?? embedInstruction.Index
                let embedValue = embedInstruction.GetEmbedValue(value)
                let jEmbedValue = embedValue is null
                    ? JValue.CreateNull()
                    : Visit(embedValue, embedInstruction.Type)
                let jProperty = new JProperty(embedInstruction.Relation, jEmbedValue)
                select (index, jProperty);
            foreach (var (index, jProperty) in embedPairs)
            {
                embeds = embeds.Add(jProperty);
                /* note(cosborn)
                 * Remember, indexing the JToken will give us a value within
                 * a JProperty. We can't remove the value from the property
                 * (what would that mean?), so we move up one level to remove
                 * the entire property from the parent object.
                 */
                jObject[index]?.Parent.Remove();
            }

            if (embeds.Count != 0)
            {
                jObject[EmbeddedKey] = new JObject(embeds);
            }

            return jObject;
        }

        [NotNull]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = @"Ending with ""Contract"" is OK.")]
        JsonObjectContract CreateObjectContract([NotNull] Type type) =>
            (JsonObjectContract)_cocInfo.Invoke(SerializerSettings.ContractResolver, new object[] { type });
    }
}
