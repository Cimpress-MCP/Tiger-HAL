using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Tiger.Hal
{
    /// <summary>
    /// Builds an <see cref="IHalRepository"/> from <see cref="HalProfile"/>s.
    /// </summary>
    [UsedImplicitly(Members)]
    sealed class HalRepositoryBuilder
    {
        readonly List<HalProfile> _profiles = new List<HalProfile>();

        /// <summary>Adds the given profile to the builder.</summary>
        /// <typeparam name="TProfile">The type of the profile.</typeparam>
        /// <param name="profile">The profile to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="profile"/> is <see langword="null"/>.</exception>
        public void AddProfile<TProfile>([NotNull] TProfile profile)
            where TProfile : HalProfile
        {
            if (profile == null) { throw new ArgumentNullException(nameof(profile)); }

            _profiles.Add(profile);
        }

        /// <summary>Adds an instance of the given profile type to the builder.</summary>
        /// <typeparam name="TProfile">The type of the profile to construct.</typeparam>
        public void AddProfile<TProfile>()
            where TProfile : HalProfile, new() => AddProfile(new TProfile());

        /// <summary>Adds an instance of the given profile type to the builder.</summary>
        /// <param name="profileType">
        /// The type of the profile, which must be a concrete, constructed type with a parameterless constructor.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="profileType"/> is <see langword="null"/>.</exception>
        /// <exception cref="MissingMethodException"><paramref name="profileType"/> does not have a parameterless constructor.</exception>
        /// <exception cref="TargetInvocationException">The constructor being called throws an exception.</exception>
        /// <exception cref="MethodAccessException">The caller does not have permission to call this constructor.</exception>
        /// <exception cref="MemberAccessException"><paramref name="profileType"/> is an abstract class.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="profileType"/> is not a runtime type, or is an open generic type
        /// (that is, <see cref="Type.IsConstructedGenericType" /> returns <see langword="false"/>).
        /// </exception>
        /// <exception cref="TypeLoadException"><paramref name="profileType"/> is not a valid type.</exception>
        [SuppressMessage(
            "ReSharper",
            "ExceptionNotDocumented",
            Justification = "If someone loads a COM type in here, they deserve what they get.")]
        public void AddProfile([NotNull] Type profileType)
        {
            if (profileType == null) { throw new ArgumentNullException(nameof(profileType)); }

            var profile = (HalProfile)Activator.CreateInstance(profileType);
            _profiles.Add(profile);
        }

        /// <summary>Builds an <see cref="IHalRepository"/>.</summary>
        /// <param name="actionContextAccessor">The application's accessor for <see cref="ActionContext"/>.</param>
        /// <param name="urlHelperFactory">The application's factory for <see cref="IUrlHelper"/>.</param>
        /// <returns>The built <see cref="IHalRepository"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="actionContextAccessor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="urlHelperFactory"/> is <see langword="null"/>.</exception>
        [NotNull]
        public IHalRepository Build(
            [NotNull] IActionContextAccessor actionContextAccessor,
            [NotNull] IUrlHelperFactory urlHelperFactory)
        {
            if (actionContextAccessor == null) { throw new ArgumentNullException(nameof(actionContextAccessor)); }
            if (urlHelperFactory == null) { throw new ArgumentNullException(nameof(urlHelperFactory)); }
            var transformations = _profiles
                .SelectMany(p => p.TransformationMaps)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return new HalRepository(actionContextAccessor, urlHelperFactory, transformations);
        }
    }
}
