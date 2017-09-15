using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Represents an instruction for creating a hoist in a HAL response.</summary>
    public interface IHoistInstruction
    {
        /// <summary>Gets the path into the object to select the value to hoist.</summary>
        [NotNull]
        string Name { get; }

        /// <summary>Retrieves the value to hoist from the given main object.</summary>
        /// <param name="main">The main object.</param>
        /// <returns>The value to hoist.</returns>
        [NotNull]
        object GetHoistValue([NotNull] object main);
    }
}
