// <copyright file="ConstantLinkInstruction.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents a link instruction represented by an unwrapped <see cref="Uri"/>.</summary>
    /// <typeparam name="T">The type of the object being linked.</typeparam>
    sealed class ConstantLinkInstruction<T>
        : ILinkInstruction
    {
        readonly Func<T, Uri> _selector;

        /// <summary>Initializes a new instance of the <see cref="ConstantLinkInstruction{T}"/> class.</summary>
        /// <param name="selector">
        /// A function that creates an <see cref="ILinkData"/> from a value of type <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
        public ConstantLinkInstruction([NotNull] Func<T, Uri> selector)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        /// <inheritdoc/>
        public bool IsSingular { get; } = true;

        /// <inheritdoc/>
        public IEnumerable<ILinkData> TransformToLinkBuilders(object main)
        {
            var link = _selector((T)main);
            if (link != null)
            {
                yield return LinkData.Const(link);
            }
        }
    }
}