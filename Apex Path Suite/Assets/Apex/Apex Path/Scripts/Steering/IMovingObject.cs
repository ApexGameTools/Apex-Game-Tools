/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using UnityEngine;

    /// <summary>
    /// Interface for objects on the move
    /// </summary>
    public interface IMovingObject
    {
        /// <summary>
        /// Gets the velocity of the object. This represents the movement force applied to the object. Also see <see cref="actualVelocity"/>.
        /// </summary>
        /// <value>
        /// The velocity.
        /// </value>
        Vector3 velocity { get; }

        /// <summary>
        /// Gets the actual velocity of the object. This may differ from <see cref="velocity"/> in certain scenarios, e.g. during collisions, if being moved by other means etc.
        /// </summary>
        /// <value>
        /// The actual velocity.
        /// </value>
        Vector3 actualVelocity { get; }
        
        /// <summary>
        /// Gets a value indicating whether this object is grounded, i.e. not falling or otherwise raised above its natural base position.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grounded; otherwise, <c>false</c>.
        /// </value>
        bool isGrounded { get; }

        /// <summary>
        /// Stops the object's movement.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits the specified seconds before continuing the move.
        /// </summary>
        /// <param name="seconds">The seconds to wait or null to wait until explicitly <see cref="Resume"/>d.</param>
        void Wait(float? seconds);

        /// <summary>
        /// Resumes movements after a <see cref="Wait" />.
        /// </summary>
        void Resume();
    }
}
