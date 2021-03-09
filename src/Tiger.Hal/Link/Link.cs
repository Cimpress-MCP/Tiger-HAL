// <copyright file="Link.cs" company="Cimpress, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static Newtonsoft.Json.DefaultValueHandling;
using static Newtonsoft.Json.Required;

namespace Tiger.Hal
{
    /// <summary>Represents a relationship between resources.</summary>
    [JsonObject(
        NamingStrategyType = typeof(CamelCaseNamingStrategy),
        NamingStrategyParameters = new object[] { false, true })]
    public sealed class Link
    {
        /// <summary>Initializes a new instance of the <see cref="Link"/> class.</summary>
        /// <param name="href">The location of the target resource.</param>
        /// <param name="isTemplated">
        /// A value indicating whether <paramref name="href"/> indicates a template which,
        /// when bound, will be the location of the location of the target resource.
        /// </param>
        /// <param name="type">
        /// A hint to indicate the media type expected when dereferencing the target resource.
        /// </param>
        /// <param name="deprecation">
        /// Whether the link has been deprecated � that is,
        /// whether it will be removed at a future date.
        /// </param>
        /// <param name="name">
        /// A secondary key for selecting links which share the same relation type.
        /// </param>
        /// <param name="profile">A hint about the profile of the target resource.</param>
        /// <param name="title">A human-readable identifier for the link.</param>
        /// <param name="hrefLang">The language of the target resource.</param>
        [JsonConstructor]
        internal Link(
            string href,
            [JsonProperty("Templated")] bool isTemplated,
            string? type,
            Uri? deprecation,
            string? name,
            Uri? profile,
            string? title,
            string? hrefLang)
        {
            Href = href;
            IsTemplated = isTemplated;
            Type = type;
            Deprecation = deprecation;
            Name = name;
            Profile = profile;
            Title = title;
            HrefLang = hrefLang;
        }

        /// <summary>
        /// Gets the location of the target resource or a template which, when bound,
        /// will be the location of the location of the target resource.
        /// </summary>
        [JsonProperty(Required = Always)]
        public string Href { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Href"/> is a template which, when bound,
        /// will be the location of the location of the target resource.
        /// </summary>
        [JsonProperty("Templated", DefaultValueHandling = Ignore)]
        public bool IsTemplated { get; }

        /// <summary>
        /// Gets a hint to indicate the media type expected
        /// when dereferencing the target resource.
        /// </summary>
        [JsonProperty(DefaultValueHandling = Ignore)]
        [SuppressMessage("Microsoft:Guidelines", "CA1721", Justification = "That's what it's called.")]
        public string? Type { get; }

        /// <summary>
        /// Gets whether the link has been deprecated – that is, whether
        /// it will be removed at a future date.
        /// </summary>
        [JsonProperty(DefaultValueHandling = Ignore)]
        public Uri? Deprecation { get; }

        /// <summary>
        /// Gets a secondary key for selecting links which
        /// share the same relation type.
        /// </summary>
        [JsonProperty(DefaultValueHandling = Ignore)]
        public string? Name { get; }

        /// <summary>Gets a hint about the profile of the target resource.</summary>
        [JsonProperty(DefaultValueHandling = Ignore)]
        public Uri? Profile { get; }

        /// <summary>Gets a human-readable identifier for the link.</summary>
        [JsonProperty(DefaultValueHandling = Ignore)]
        public string? Title { get; }

        /// <summary>Gets the language of the target resource.</summary>
        [JsonProperty("Hreflang", DefaultValueHandling = Ignore)]
        public string? HrefLang { get; }
    }
}
