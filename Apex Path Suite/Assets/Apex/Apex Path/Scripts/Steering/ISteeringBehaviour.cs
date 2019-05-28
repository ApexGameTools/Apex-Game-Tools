/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    /// <summary>
    /// Interface for components that influence the steering of a unit.
    /// </summary>
    public interface ISteeringBehaviour
    {
        /// <summary>
        /// Gets the priority of this steering behaviour relative to others. Only behaviours with the highest priority will influence the steering of the unit, provided they have any steering output.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        int priority { get; }

        /// <summary>
        /// Gets the steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        void GetSteering(SteeringInput input, SteeringOutput output);

        /// <summary>
        /// Stops this steering behaviour.
        /// </summary>
        void Stop();
    }
}
