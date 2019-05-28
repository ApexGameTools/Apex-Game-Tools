/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    /// <summary>
    /// A height navigator for maps with no height.
    /// </summary>
    public sealed class NoHeightNavigator : IHeightNavigator
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static readonly NoHeightNavigator Instance = new NoHeightNavigator();

        /// <summary>
        /// Gets the gravity acceleration.
        /// </summary>
        public float gravity
        {
            get { return 0f; }
        }

        /// <summary>
        /// Gets the height output.
        /// </summary>
        /// <param name="input">The steering input.</param>
        /// <param name="effectiveMaxSpeed">The effective maximum speed of the unit, this may be higher than the desired speed.</param>
        /// <returns>
        /// The height output
        /// </returns>
        public HeightOutput GetHeightOutput(SteeringInput input, float effectiveMaxSpeed)
        {
            return new HeightOutput
            {
                finalVelocity = input.currentSpatialVelocity,
                isGrounded = true
            };
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="other">The component to clone from.</param>
        public void CloneFrom(IHeightNavigator other)
        {
            /* NOOP */
        }
    }
}
