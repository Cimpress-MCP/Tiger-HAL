// <copyright file="Link.NamingStrategy.cs" company="Cimpress, Inc.">
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

using Newtonsoft.Json.Serialization;

namespace Tiger.Hal
{
    /// <content>JSON serialization naming strategy.</content>
    public sealed partial class Link
    {
        /// <summary>
        /// Defines the strategy for naming keys in the JSON serialization of <see cref="Link"/>.
        /// </summary>
        sealed class NamingStrategy
            : CamelCaseNamingStrategy
        {
            /// <inheritdoc/>
            public override string GetPropertyName(string name, bool hasSpecifiedName)
            {
                switch (name)
                {
                    case nameof(IsTemplated):
                        return "templated";
                    case nameof(HrefLang):
                        return "hreflang";
                    default:
                        return base.GetPropertyName(name, hasSpecifiedName);
                }
            }
        }
    }
}
