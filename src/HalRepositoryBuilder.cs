// <copyright file="HalRepositoryBuilder.cs" company="Cimpress, Inc.">
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Tiger.Hal
{
    /// <summary>Builds an <see cref="IHalRepository"/> from <see cref="IHalProfile"/>s.</summary>
    [UsedImplicitly(Members)]
    sealed class HalRepositoryBuilder
    {
        readonly IActionContextAccessor _actionContextAccessor;
        readonly IUrlHelperFactory _urlHelperFactory;

        /// <summary>Initializes a new instance of the <see cref="HalRepositoryBuilder"/> class.</summary>
        /// <param name="actionContextAccessor">The application's accessor for <see cref="ActionContext"/>.</param>
        /// <param name="urlHelperFactory">The application's factory for <see cref="IUrlHelper"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="actionContextAccessor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="urlHelperFactory"/> is <see langword="null"/>.</exception>
        public HalRepositoryBuilder(
            [NotNull] IActionContextAccessor actionContextAccessor,
            [NotNull] IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _urlHelperFactory = urlHelperFactory ?? throw new ArgumentNullException(nameof(urlHelperFactory));
        }

        /// <summary>Builds an <see cref="IHalRepository"/>.</summary>
        /// <param name="profile">
        /// The profile containing the declarations for creating a transformation mapping.
        /// </param>
        /// <returns>The built <see cref="IHalRepository"/>.</returns>
        [NotNull]
        public IHalRepository Build([NotNull] IHalProfile profile)
        {
            var transformationMap = new TransformationMap();
            profile.OnTransformationMapCreating(transformationMap);
            var transformations = transformationMap.Maps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return new HalRepository(_actionContextAccessor, _urlHelperFactory, transformations);
        }
    }
}
