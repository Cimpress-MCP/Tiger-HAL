using JetBrains.Annotations;

namespace Tiger.Hal
{
    /// <summary>Defines the HAL+JSON transformations for an application.</summary>
    [PublicAPI]
    public interface IHalProfile
    {
        /// <summary>Configures the transformation from a type to its HAL+JSON representation.</summary>
        /// <param name="transformationMap">The application's map of HAL transformation.</param>
        void OnTransformationMapCreating([NotNull] TransformationMap transformationMap);
    }
}
