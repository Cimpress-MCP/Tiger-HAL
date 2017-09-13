using System;
using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Maps types to type transformers.</summary>
    public interface IHalRepository
    {
        /// <summary>
        /// Determines whether a type has a registered transformer.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="type"/> has a registered transformer,
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool CanTransform([NotNull] Type type);

        /// <summary>
        /// Gets the transformer that is associated with the given type.
        /// </summary>
        /// <param name="type">The type to locate.</param>
        /// <param name="transformer">
        /// When this method returns, the transformer associated with the given type, if the key is found;
        /// otherwise, the default value of <see cref="ITypeTransformer"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if there is a transformer associated with <paramref name="type"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        [ContractAnnotation("=> true, transformer:notnull; => false, transformer:null")]
        bool TryGetTransformer([NotNull] Type type, out ITypeTransformer transformer);
    }
}
