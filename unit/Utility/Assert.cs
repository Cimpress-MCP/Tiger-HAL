using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Xunit.Sdk;
using static JetBrains.Annotations.ImplicitUseKindFlags;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    [UsedImplicitly(InstantiatedNoFixedConstructorSignature, WithMembers)]
#if XUNIT_VISIBILITY_INTERNAL
    internal
#else
    public
#endif
    partial class Assert
    {
        /// <summary>Verifies that a dictionary contains a given key.</summary>
        /// <typeparam name="TKey">The type of the key to be verified.</typeparam>
        /// <typeparam name="TValue">The type of the value associated with the keys.</typeparam>
        /// <param name="expected">The key expected to be in the dictionary.</param>
        /// <param name="collection">The dictionary to be inspected.</param>
        /// <returns>The value associated with <paramref name="expected"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        /// <exception cref="ContainsException">The key is not present in the dictionary.</exception>
        [CanBeNull]
        public static TValue Contains<TKey, TValue>([NotNull] TKey expected, [NotNull] IReadOnlyDictionary<TKey, TValue> collection)
        {
            if (expected == null) { throw new ArgumentNullException(nameof(expected)); }
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }

            return collection.TryGetValue(expected, out var value)
                ? value :
                throw new ContainsException(expected, collection);
        }

        /// <summary>Verifies that a dictionary contains a given key.</summary>
        /// <typeparam name="TKey">The type of the key to be verified.</typeparam>
        /// <typeparam name="TValue">The type of the value associated with the keys.</typeparam>
        /// <param name="expected">The key expected to be in the dictionary.</param>
        /// <param name="collection">The dictionary to be inspected.</param>
        /// <returns>The value associated with <paramref name="expected"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="expected"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        /// <exception cref="ContainsException">The key is not present in the dictionary.</exception>
        [CanBeNull]
        public static TValue Contains<TKey, TValue>([NotNull] TKey expected, [NotNull] IDictionary<TKey, TValue> collection)
        {
            if (expected == null) { throw new ArgumentNullException(nameof(expected)); }
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }

            return collection.TryGetValue(expected, out var value)
                ? value
                : throw new ContainsException(expected, collection);
        }
    }
}
