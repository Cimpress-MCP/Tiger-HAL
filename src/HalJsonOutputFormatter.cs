using System;
using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Tiger.Hal
{
    /// <summary>A <see cref="JsonOutputFormatter"/> for HAL+JSON content.</summary>
    [PublicAPI]
    public sealed class HalJsonOutputFormatter
        : JsonOutputFormatter
    {
        readonly IHalRepository _halRepository;

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
        JToken WalkObject( // ReSharper disable once InconsistentNaming
            [NotNull] JsonObjectContract jsonObjectContract,
            [CanBeNull] JToken jValue,
            [CanBeNull] object value)
        {
            if (value == null || jValue == null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonObjectContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jValue;
            }

            foreach (var property in jsonObjectContract.Properties)
            {
                var jPropertyValue = jValue[property.PropertyName];
                var nativeValue = property.ValueProvider.GetValue(value);
                switch (SerializerSettings.ContractResolver.ResolveContract(property.PropertyType))
                {
                    case JsonObjectContract joc:
                        jValue[property.PropertyName] = WalkObject(joc, jPropertyValue, nativeValue);
                        break;
                    case JsonArrayContract jac:
                        jValue[property.PropertyName] = WalkArray(jac, jPropertyValue, nativeValue);
                        break;
                    case JsonDictionaryContract jdc:
                        jValue[property.PropertyName] = WalkDictionary(jdc, jPropertyValue, nativeValue);
                        break;

                    // note(cosborn) Dynamic? Something else?
                }
            }

            var links = transformer.GenerateLinks(value);
            if (links.Count != 0)
            {
                jValue["_links"] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableList.Create<JProperty>();
            foreach (var embedInstruction in transformer.Embeds)
            {
                embeds = embeds.Add(new JProperty(
                    embedInstruction.Relation,
                    Walk(embedInstruction.GetEmbeddedValue(value), embedInstruction.Type)));
                var embedProperty = jsonObjectContract.Properties
                    .SingleOrDefault(p => p.UnderlyingName == embedInstruction.Index.ToString());
                /* note(cosborn)
                 * Remember, indexing the JToken will give us a value within
                 * a JProperty. We can't remove the value from the property
                 * (what would that mean?), so we move up one level to remove
                 * the entire property from the parent object.
                 */
                jValue[embedProperty?.PropertyName ?? embedInstruction.Index]?.Parent.Remove();
            }

            if (embeds.Count != 0)
            {
                jValue["_embedded"] = new JObject(embeds);
            }

            return jValue;
        }

        [CanBeNull]
        JToken WalkArray( // ReSharper disable once InconsistentNaming
            [NotNull] JsonArrayContract jsonArrayContract,
            [CanBeNull] JToken jValue,
            [CanBeNull] object value)
        {
            if (value == null || jValue == null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonArrayContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jValue;
            }

            var arrayValue = ((IEnumerable)value).Cast<object>().ToImmutableArray(); // ReSharper disable once InconsistentNaming
            var collectionItemContract = SerializerSettings.ContractResolver.ResolveContract(jsonArrayContract.CollectionItemType);
            foreach (var (index, indexValue) in arrayValue.Select((o, i) => (index: i, indexValue: o)))
            {
                switch (collectionItemContract)
                {
                    case JsonObjectContract joc:
                        jValue[index] = WalkObject(joc, jValue[index], indexValue);
                        break;
                    case JsonArrayContract jac:
                        jValue[index] = WalkArray(jac, jValue[index], indexValue);
                        break;
                    case JsonDictionaryContract jdc:
                        jValue[index] = WalkDictionary(jdc, jValue[index], indexValue);
                        break;

                    // note(cosborn) Dynamic? Something else?
                }
            }

            // note(cosborn) Lists embed themselves in a wrapper object.
            var wrapperObject = JObject.FromObject(
                new
                { // note(cosborn) Indirect through JObject for proper key name transformation.
                    Count = arrayValue.Length
                }, CreateJsonSerializer());

            var links = transformer.GenerateLinks(value);
            if (links.Count != 0)
            {
                wrapperObject["_links"] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableList.Create<JProperty>();
            foreach (var embedInstruction in transformer.Embeds)
            {
                embeds = embeds.Add(new JProperty(
                    embedInstruction.Relation,
                    Walk(embedInstruction.GetEmbeddedValue(value), embedInstruction.Type)));
                if (embedInstruction.Index is int index)
                { // Json.NET will panic if we send in anything but an int.
                    jValue[index]?.Remove();
                }
            }

            // note(cosborn) We know embeds has at least one.
            wrapperObject["_embedded"] = new JObject(embeds.Add(new JProperty("self", jValue)));

            return wrapperObject;
        }

        [CanBeNull]
        JToken WalkDictionary( // ReSharper disable once InconsistentNaming
            [NotNull] JsonDictionaryContract jsonDictionaryContract,
            [CanBeNull] JToken jValue,
            [CanBeNull] object value)
        {
            if (value == null || jValue == null) { return null; }

            if (!_halRepository.TryGetTransformer(jsonDictionaryContract.UnderlyingType, out var transformer))
            { // note(cosborn) No setup, no transformation.
                return jValue;
            }

            var dictValue = (IDictionary)value; // ReSharper disable once InconsistentNaming
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

                    // note(cosborn) Dynamic? Something else?
                }
            }

            var links = transformer.GenerateLinks(value);
            if (links.Count != 0)
            {
                jValue["_links"] = JObject.FromObject(links, CreateJsonSerializer());
            }

            var embeds = ImmutableList.Create<JProperty>();
            foreach (var embedInstruction in transformer.Embeds)
            {
                embeds = embeds.Add(new JProperty(
                    embedInstruction.Relation,
                    Walk(embedInstruction.GetEmbeddedValue(value), embedInstruction.Type)));
                var embedKey = jsonDictionaryContract.DictionaryKeyResolver(embedInstruction.Index.ToString());
                /* note(cosborn)
                 * Remember, indexing the JToken will give us a value within
                 * a JProperty. We can't remove the value from the property
                 * (what would that mean?), so we move up one level to remove
                 * the entire property from the parent object.
                 */
                jValue[embedKey ?? embedInstruction.Index]?.Parent.Remove();
            }

            if (embeds.Count != 0)
            {
                jValue["_embedded"] = new JObject(embeds);
            }

            return jValue;
        }
    }
}
