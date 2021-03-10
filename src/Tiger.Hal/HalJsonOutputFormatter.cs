// <copyright file="HalJsonOutputFormatter.cs" company="Cimpress, Inc.">
//   Copyright 2020 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static System.Reflection.BindingFlags;

namespace Tiger.Hal
{
    /// <summary>A <see cref="TextOutputFormatter"/> for HAL+JSON content.</summary>
    public sealed class HalJsonOutputFormatter
        : NewtonsoftJsonOutputFormatter
    {
        const string LinksKey = "_links";
        const string EmbeddedKey = "_embedded";

        readonly IHalRepository _halRepository;

        // note(cosborn) Cache the reflection; it's relatively expensive.
        readonly MethodInfo? _cocInfo = typeof(DefaultContractResolver).GetMethod("CreateObjectContract", Instance | NonPublic);

        /// <summary>Initializes a new instance of the <see cref="HalJsonOutputFormatter"/> class.</summary>
        /// <param name="serializerSettings">
        /// The <see cref="JsonSerializerSettings"/>. Should be either the application-wide settings
        /// (<see cref="MvcNewtonsoftJsonOptions.SerializerSettings"/>) or an instance
        /// <see cref="JsonSerializerSettingsProvider.CreateSerializerSettings"/> initially returned.
        /// </param>
        /// <param name="charPool">The <see cref="ArrayPool{T}"/>.</param>
        /// <param name="mvcOptions">The application's option for configuring MVC.</param>
        /// <param name="halRepository">The application's HAL+JSON repository.</param>
        public HalJsonOutputFormatter(
            JsonSerializerSettings serializerSettings,
            ArrayPool<char> charPool,
            MvcOptions mvcOptions,
            IHalRepository halRepository)
            : base(serializerSettings, charPool, mvcOptions)
        {
            _halRepository = halRepository;

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add("application/hal+json");
        }

        /// <inheritdoc/>
        public override Task WriteResponseBodyAsync(
            OutputFormatterWriteContext context,
            Encoding selectedEncoding)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

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
        protected override bool CanWriteType(Type type) => _halRepository.CanTransform(type);

        JToken? Visit(object? value, Type type) => (value, SerializerSettings.ContractResolver.ResolveContract(type)) switch
        {
            (null, _) => null,
            ({ } v, JsonObjectContract joc) => VisitObject(joc, JObject.FromObject(v, CreateJsonSerializer()), v),
            ({ } v, JsonArrayContract jac) => VisitArray(jac, JArray.FromObject(v, CreateJsonSerializer()), (IEnumerable)v),
            ({ } v, JsonDictionaryContract jdc) => VisitDictionary(jdc, JObject.FromObject(v, CreateJsonSerializer()), (IDictionary)v),
            ({ } v, _) => JToken.FromObject(v, CreateJsonSerializer()),
        };

        JToken VisitObject(
            JsonObjectContract jsonObjectContract,
            JObject jObject,
            object value)
        {
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
                switch ((propertyValueContract, type: jPropertyValue.Type, nativeValue))
                {
                    case (_, _, null):
                        jObject[name] = JValue.CreateNull();
                        break;
                    case (JsonObjectContract joc, JTokenType.Object, { } nv):
                        jObject[name] = VisitObject(joc, (JObject)jPropertyValue, nv);
                        break;
                    case (JsonArrayContract jac, JTokenType.Array, { } nv):
                        jObject[name] = VisitArray(jac, (JArray)jPropertyValue, (IEnumerable)nv);
                        break;
                    case (JsonDictionaryContract jdc, JTokenType.Object, { } nv):
                        jObject[name] = VisitDictionary(jdc, (JObject)jPropertyValue, (IDictionary)nv);
                        break;

                        // todo(cosborn) Dynamic? Something else?
                }
            }

            var links = transformer.GenerateLinks(value);
            if (links.Count != 0)
            {
                jObject[LinksKey] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableList<JProperty>.Empty;
            var embedPairs =
                from embedInstruction in transformer.Embeds
                join property in jsonObjectContract.Properties
                    on embedInstruction.Index equals property.UnderlyingName
                let jEmbedValue = embedInstruction.GetEmbedValue(value, Visit)
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

        JToken VisitArray(
            JsonArrayContract jsonArrayContract,
            JArray jArray,
            IEnumerable value)
        {
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
                switch ((collectionItemContract, type: jIndexValue.Type))
                {
                    case (JsonObjectContract joc, JTokenType.Object):
                        jArray[index] = VisitObject(joc, (JObject)jIndexValue, nativeValue);
                        break;
                    case (JsonArrayContract jac, JTokenType.Array):
                        jArray[index] = VisitArray(jac, (JArray)jIndexValue, (IEnumerable)nativeValue);
                        break;
                    case (JsonDictionaryContract jdc, JTokenType.Object):
                        jArray[index] = VisitDictionary(jdc, (JObject)jIndexValue, (IDictionary)nativeValue);
                        break;

                        // todo(cosborn) Dynamic? Something else?
                }
            }

            // note(cosborn) Lists embed themselves in a wrapper object.
            var wrapperObject = new JObject();

            var links = transformer.GenerateLinks(value).ToImmutableDictionary();
            if (links.Count != 0)
            {
                wrapperObject[LinksKey] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableArray<JProperty>.Empty;
            var embedProperties =
                from embedInstruction in transformer.Embeds
                let jEmbedValue = embedInstruction.GetEmbedValue(value, Visit)
                let jProperty = new JProperty(embedInstruction.Relation, jEmbedValue)
                select jProperty;
            foreach (var jProperty in embedProperties)
            {
                embeds = embeds.Add(jProperty);
            }

            if (embeds.Length != 0)
            {
                wrapperObject[EmbeddedKey] = new JObject(embeds);
            }

            // note(cosborn) Check count because object contract creation is relatively expensive due to reflection.
            if (transformer.Hoists.Count != 0)
            {
                var objectContract = CreateObjectContract(jsonArrayContract.UnderlyingType);
                if (objectContract is null)
                {
                    throw new InvalidOperationException("Could not create object contract!");
                }

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

        JToken VisitDictionary(
            JsonDictionaryContract jsonDictionaryContract,
            JObject jObject,
            IDictionary value)
        {
            if (!_halRepository.TryGetTransformer(jsonDictionaryContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jObject;
            }

            var dictionaryValueContract = SerializerSettings.ContractResolver.ResolveContract(jsonDictionaryContract.DictionaryValueType);
            foreach (var key in value.Keys.Cast<object>().Where(k => k is not null))
            {
                var jKey = jsonDictionaryContract.DictionaryKeyResolver(key.ToString());
                var jValue = jObject[jKey];
                switch ((dictionaryValueContract, type: jValue.Type, value[key]))
                {
                    case (_, _, null):
                        jObject[jKey] = JValue.CreateNull();
                        break;
                    case (JsonObjectContract joc, JTokenType.Object, { } v):
                        jObject[jKey] = new JProperty(jKey, VisitObject(joc, (JObject)jObject[jKey], v));
                        break;
                    case (JsonArrayContract jac, JTokenType.Array, { } v):
                        jObject[jKey] = new JProperty(jKey, VisitArray(jac, (JArray)jObject[jKey], (IEnumerable)v));
                        break;
                    case (JsonDictionaryContract jdc, JTokenType.Object, { } v):
                        jObject[jKey] = new JProperty(jKey, VisitDictionary(jdc, (JObject)jObject[jKey], (IDictionary)v));
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
                let embedKey = jsonDictionaryContract.DictionaryKeyResolver(embedInstruction.Index)
                let index = embedKey ?? embedInstruction.Index
                let jEmbedValue = embedInstruction.GetEmbedValue(value, Visit)
                let jProperty = new JProperty(embedInstruction.Relation, jEmbedValue)
                select (index, jProperty);
            foreach (var (index, jProperty) in embedPairs)
            {
                embeds = embeds.Add(jProperty);
                /* note(cosborn)
                 * Remember, indexing the JObject will give us a value within
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

        JsonObjectContract? CreateObjectContract(Type type) =>
            _cocInfo?.Invoke(SerializerSettings.ContractResolver, new object[] { type }) as JsonObjectContract;
    }
}
