/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    /// <summary>
    /// Interface for height navigators.
    /// </summary>
    public interface IHeightNavigator
    {
        /// <summary>
        /// Gets the gravity acceleration.
        /// </summary>
        float gravity { get; }

        /// <summary>
        /// Gets the height output.
        /// </summary>
        /// <param name="input">The steering input.</param>
        /// <param name="effectiveMaxSpeed">The effective maximum speed of the unit, this may be higher than the desired speed.</param>
        /// <returns>The height output</returns>
        HeightOutput GetHeightOutput(SteeringInput input, float effectiveMaxSpeed);

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="other">The component to clone from.</param>
        void CloneFrom(IHeightNavigator other);
    }
}
