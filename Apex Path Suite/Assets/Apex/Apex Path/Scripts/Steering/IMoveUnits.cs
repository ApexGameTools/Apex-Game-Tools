/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using UnityEngine;

    /// <summary>
    /// Interface for custom unit movement implementations. Implement this if you want to apply your own movement logic to the <see cref="SteerableUnitComponent"/>.
    /// </summary>
    public interface IMoveUnits
    {
        /// <summary>
        /// Moves the unit by the specified velocity.
        /// </summary>
        /// <param name="velocity">The velocity.</param>
        /// <param name="deltaTime">Time since last invocation in ms</param>
        void Move(Vector3 velocity, float deltaTime);

        /// <summary>
        /// Rotates the unit.
        /// </summary>
        /// <param name="targetOrientation">The target orientation.</param>
        /// <param name="angularSpeed">The angular speed.</param>
        /// <param name="deltaTime">Time since last invocation in ms.</param>
        void Rotate(Vector3 targetOrientation, float angularSpeed, float deltaTime);

        /// <summary>
        /// Stops the unit.
        /// </summary>
        void Stop();
    }
}
