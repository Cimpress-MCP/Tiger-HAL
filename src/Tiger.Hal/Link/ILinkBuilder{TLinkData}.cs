// <copyright file="ILinkBuilder{TLinkData}.cs" company="Cimpress, Inc.">
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

namespace Tiger.Hal
{
    /// <summary>Transforms implementations of <see cref="ILinkData"/> to <see cref="Link"/>.</summary>
    /// <typeparam name="TLinkData">The type of link data.</typeparam>
    public interface ILinkBuilder<in TLinkData>
        where TLinkData : ILinkData
    {
        /// <summary>
        /// Builds a <see cref="Link"/> from the given link data.
        /// </summary>
        /// <param name="linkData">The data from which to build a link.</param>
        /// <returns>A link.</returns>
        Link Build(TLinkData linkData);
    }
}
