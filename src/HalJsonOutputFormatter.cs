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

namespace Tiger.Hal
{
    /// <summary>A <see cref="JsonOutputFormatter"/> for HAL+JSON content.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = @"Ending with ""Contract"" is OK.")]
    [PublicAPI]
    public sealed class HalJsonOutputFormatter
        : JsonOutputFormatter
    {
        readonly IHalRepository _halRepository;

        // note(cosborn) Cache the reflection; it's relatively expensive.
        readonly MethodInfo _cocInfo = typeof(DefaultContractResolver)
            .GetMethod("CreateObjectContract", Instance | NonPublic);

        /// <summary>Initializes a new instance of the <see cref="HalJsonOutputFormatter"/> class.</summary>
        /// <param name="halRepository">The application's HAL+JSON repository.</param>
        /// <param name="serializerSettings">
        /// The <see cref="JsonSerializerSettings"/>. Should be either the application-wide settings
        /// (<see cref="MvcJsonOptions.SerializerSettings"/>) or an instance
        /// <see cref="JsonSerializerSettingsProvider.CreateSerializerSettings"/> initially returned.
        /// </param>
        /// <param name="charPool">The <see cref="ArrayPool{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="halRepository"/> is <see langword="null"/>.</exception>
        public HalJsonOutputFormatter(
            [NotNull] IHalRepository halRepository,
            [NotNull] JsonSerializerSettings serializerSettings,
            [NotNull] ArrayPool<char> charPool)
            : base(serializerSettings, charPool)
        {
            _halRepository = halRepository ?? throw new ArgumentNullException(nameof(halRepository));

            SupportedMediaTypes.Add("application/hal+json");
        }

        /// <inheritdoc/>
        public override Task WriteResponseBodyAsync(
            [NotNull] OutputFormatterWriteContext context,
            Encoding selectedEncoding)
        {
            if (context.Object == null)
            { // note(cosborn) What else are we supposed to do here, huh?
                return base.WriteResponseBodyAsync(context, selectedEncoding);
            }

            var halValue = Walk(context.Object, context.ObjectType);

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
        JToken Walk(
            [NotNull] object value,
            [NotNull] Type type)
        {
            switch (SerializerSettings.ContractResolver.ResolveContract(type))
            {
                case JsonObjectContract joc:
                    return WalkObject(joc, JObject.FromObject(value, CreateJsonSerializer()), value);
                case JsonArrayContract jac:
                    return WalkArray(jac, JArray.FromObject(value, CreateJsonSerializer()), value);
                case JsonDictionaryContract jdc: // note(cosborn) Don't support defining these yet, but sure why not
                    return WalkDictionary(jdc, JObject.FromObject(value, CreateJsonSerializer()), value);
                default: // todo(cosborn) Dynamic? Something else?
                    return JToken.FromObject(value, CreateJsonSerializer());
            }
        }

        [CanBeNull]
        JToken WalkObject(
            [NotNull] JsonObjectContract jsonObjectContract,
            [CanBeNull] JToken jValue,
            [CanBeNull] object value)
        {
            if (value == null || jValue == null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonObjectContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jValue;
            }

            var walkQuads =
                from property in jsonObjectContract.Properties
                let jPropertyValue = jValue[property.PropertyName]
                where jPropertyValue != null
                let nativeValue = property.ValueProvider.GetValue(value)
                let contract = SerializerSettings.ContractResolver.ResolveContract(property.PropertyType)
                select (name: property.PropertyName, jPropertyValue, nativeValue, contract);
            foreach (var (name, jPropertyValue, nativeValue, contract) in walkQuads)
            {
                switch (contract)
                {
                    case JsonObjectContract joc:
                        jValue[name] = WalkObject(joc, jPropertyValue, nativeValue);
                        break;
                    case JsonArrayContract jac:
                        jValue[name] = WalkArray(jac, jPropertyValue, nativeValue);
                        break;
                    case JsonDictionaryContract jdc:
                        jValue[name] = WalkDictionary(jdc, jPropertyValue, nativeValue);
                        break;

                    // todo(cosborn) Dynamic? Something else?
                }
            }

            var links = transformer.GenerateLinks(value);
            if (links.Count != 0)
            {
                jValue["_links"] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableList.Create<JProperty>();
            var embedPairs =
                from embedInstruction in transformer.Embeds
                let embedIndex = embedInstruction.Index.ToString()
                join property in jsonObjectContract.Properties
                    on embedIndex equals property.UnderlyingName
                let embedValue = embedInstruction.GetEmbedValue(value)
                let jEmbedValue = embedValue == null
                    ? JValue.CreateNull()
                    : Walk(embedValue, embedInstruction.Type)
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
                jValue[index]?.Parent.Remove();
            }

            if (embeds.Count != 0)
            {
                jValue["_embedded"] = new JObject(embeds);
            }

            return jValue;
        }

        [CanBeNull]
        JToken WalkArray(
            [NotNull] JsonArrayContract jsonArrayContract,
            [CanBeNull] JToken jValue,
            [CanBeNull] object value)
        {
            if (value == null || jValue == null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonArrayContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jValue;
            }

            var collectionItemContract = SerializerSettings.ContractResolver.ResolveContract(jsonArrayContract.CollectionItemType);
            var arrayValue = ((IEnumerable)value).Cast<object>();
            var walkPairs =
                from indexPair in arrayValue.Select((o, i) => (index: i, nativeValue: o))
                let jIndexValue = jValue[indexPair.index]
                select (indexPair, jIndexValue);
            foreach (var ((index, nativeValue), jIndexValue) in walkPairs)
            {
                switch (collectionItemContract)
                {
                    case JsonObjectContract joc:
                        jValue[index] = WalkObject(joc, jIndexValue, nativeValue);
                        break;
                    case JsonArrayContract jac:
                        jValue[index] = WalkArray(jac, jIndexValue, nativeValue);
                        break;
                    case JsonDictionaryContract jdc:
                        jValue[index] = WalkDictionary(jdc, jIndexValue, nativeValue);
                        break;

                    // todo(cosborn) Dynamic? Something else?
                }
            }

            // note(cosborn) Lists embed themselves in a wrapper object.
            var wrapperObject = new JObject();

            var links = transformer.GenerateLinks(value);
            if (links.Count != 0)
            {
                wrapperObject["_links"] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableList.Create<JProperty>();
            var embedPairs =
                from embedInstruction in transformer.Embeds
                let embedValue = embedInstruction.GetEmbedValue(value)
                let jEmbedValue = embedValue == null
                    ? JValue.CreateNull()
                    : Walk(embedValue, embedInstruction.Type)
                let jProperty = new JProperty(embedInstruction.Relation, jEmbedValue)
                select (index: embedInstruction.Index, jProperty);
            foreach (var (index, jProperty) in embedPairs)
            {
                embeds = embeds.Add(jProperty);
                if (index is int arrayIndex)
                { // note(cosborn) Json.NET will panic if we send in anything but an int.
                    jValue[arrayIndex]?.Remove();
                }
            }

            // note(cosborn) We know embeds has at least one.
            wrapperObject["_embedded"] = new JObject(embeds.Add(new JProperty("self", jValue)));

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
        JToken WalkDictionary(
            [NotNull] JsonDictionaryContract jsonDictionaryContract,
            [CanBeNull] JToken jValue,
            [CanBeNull] object value)
        {
            if (value == null || jValue == null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonDictionaryContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jValue;
            }

            var dictValue = (IDictionary)value;
            var dictionaryValueContract = SerializerSettings.ContractResolver.ResolveContract(jsonDictionaryContract.DictionaryValueType);
            foreach (var key in dictValue.Keys)
            {
                var jKey = jsonDictionaryContract.DictionaryKeyResolver(key.ToString());
                switch (dictionaryValueContract)
                {
                    case JsonObjectContract joc:
                        jValue[jKey] = new JProperty(jKey, WalkObject(joc, jValue[jKey], dictValue[key]));
                        break;
                    case JsonArrayContract jac:
                        jValue[jKey] = new JProperty(jKey, WalkArray(jac, jValue[jKey], dictValue[key]));
                        break;
                    case JsonDictionaryContract jdc:
                        jValue[jKey] = new JProperty(jKey, WalkDictionary(jdc, jValue[jKey], dictValue[key]));
                        break;

                    // todo(cosborn) Dynamic? Something else?
                }
            }

            var links = transformer.GenerateLinks(value);
            if (links.Count != 0)
            {
                jValue["_links"] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableList.Create<JProperty>();
            var embedPairs =
                from embedInstruction in transformer.Embeds
                let nativeKey = embedInstruction.Index.ToString()
                let embedKey = jsonDictionaryContract.DictionaryKeyResolver(nativeKey)
                let index = embedKey ?? embedInstruction.Index
                let embedValue = embedInstruction.GetEmbedValue(value)
                let jEmbedValue = embedValue == null
                    ? JValue.CreateNull()
                    : Walk(embedValue, embedInstruction.Type)
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
                jValue[index]?.Parent.Remove();
            }

            if (embeds.Count != 0)
            {
                jValue["_embedded"] = new JObject(embeds);
            }

            return jValue;
        }

        JsonObjectContract CreateObjectContract(Type type) =>
            (JsonObjectContract)_cocInfo.Invoke(SerializerSettings.ContractResolver, new object[] { type });
    }
}
