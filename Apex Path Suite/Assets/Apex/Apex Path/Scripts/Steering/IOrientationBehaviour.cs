/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    /// <summary>
    /// Interface for components that control the orientation of a unit.
    /// </summary>
    public interface IOrientationBehaviour
    {
        /// <summary>
        /// Gets the priority of this orientation behaviour relative to others. Only the behaviour with the highest priority will influence the orientation of the unit.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        int priority { get; }

        /// <summary>
        /// Gets the orientation output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the orientation output.</param>
        /// <param name="output">The orientation output to be populated.</param>
        void GetOrientation(SteeringInput input, OrientationOutput output);
    }
}
