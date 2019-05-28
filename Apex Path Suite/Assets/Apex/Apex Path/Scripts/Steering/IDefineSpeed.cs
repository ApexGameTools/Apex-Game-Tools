/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using UnityEngine;

    /// <summary>
    /// Interface for components that define speed for an entity type.
    /// </summary>
    public interface IDefineSpeed
    {
        /// <summary>
        /// Gets the maximum acceleration (m / s^2), i.e. how fast can the unit reach its desired speed.
        /// </summary>
        /// <value>
        /// The maximum acceleration.
        /// </value>
        float maxAcceleration { get; }

        /// <summary>
        /// Gets the maximum deceleration (m / s^2), i.e. how fast can the unit slow down.
        /// </summary>
        /// <value>
        /// The maximum deceleration.
        /// </value>
        float maxDeceleration { get; }

        /// <summary>
        /// Gets the maximum angular acceleration (rads / s^2), i.e. how fast can the unit reach its desired turn speed.
        /// </summary>
        /// <value>
        /// The maximum angular acceleration.
        /// </value>
        float maxAngularAcceleration { get; }

        /// <summary>
        /// Gets the minimum speed of the unit. Any speed below this value will mean a stop.
        /// </summary>
        /// <value>
        /// The minimum speed.
        /// </value>
        float minimumSpeed { get; }

        /// <summary>
        /// Gets the maximum speed of the unit.
        /// </summary>
        /// <value>
        /// The maximum speed.
        /// </value>
        float maximumSpeed { get; }

        /// <summary>
        /// Gets the maximum angular speed (rads / s), i.e. how fast can the unit turn.
        /// </summary>
        /// <value>
        /// The maximum angular speed.
        /// </value>
        float maximumAngularSpeed { get; }

        /// <summary>
        /// Signal that the unit has stopped.
        /// </summary>
        void SignalStop();

        /// <summary>
        /// Sets the preferred speed of the unit.
        /// </summary>
        /// <param name="speed">The speed.</param>
        void SetPreferredSpeed(float speed);

        /// <summary>
        /// Gets the preferred speed of the unit.
        /// </summary>
        /// <param name="currentMovementDirection">The current movement direction.</param>
        /// <returns>The preferred speed</returns>
        float GetPreferredSpeed(Vector3 currentMovementDirection);

        /// <summary>
        /// Clones settings from another speed component.
        /// </summary>
        /// <param name="speedComponent">The speed component to clone from.</param>
        void CloneFrom(IDefineSpeed speedComponent);
    }
}
