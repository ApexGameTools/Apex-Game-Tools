/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Orients the unit to face the direction it is moving
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer to Align with Velocity", 1033)]
    [ApexComponent("Steering")]
    public class SteerToAlignWithVelocity : OrientationComponent
    {
        /// <summary>
        /// The minimum speed to turn.
        /// </summary>
        public float minimumSpeedToTurn = 0.1f;

        /// <summary>
        /// Whether to align with the elevation, e.g. when moving up should the unit also change its y facing.
        /// </summary>
        public bool alignWithElevation;

        /// <summary>
        /// Gets the orientation.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public override void GetOrientation(SteeringInput input, OrientationOutput output)
        {
            if (input.currentPlanarVelocity.sqrMagnitude < this.minimumSpeedToTurn * this.minimumSpeedToTurn)
            {
                output.desiredAngularAcceleration = -input.currentAngularSpeed / Time.fixedDeltaTime;
                return;
            }

            var align = this.alignWithElevation && input.unit.isGrounded;

            var targetOrientation = align ? input.currentFullVelocity.normalized : input.currentPlanarVelocity.normalized;
            output.desiredOrientation = targetOrientation;
            output.desiredAngularAcceleration = GetAngularAcceleration(targetOrientation, input);
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="steerToAlign">The component to clone from.</param>
        public void CloneFrom(SteerToAlignWithVelocity steerToAlign)
        {
            this.priority = steerToAlign.priority;
            this.slowingDistance = steerToAlign.slowingDistance;
            this.slowingAlgorithm = steerToAlign.slowingAlgorithm;

            this.minimumSpeedToTurn = steerToAlign.minimumSpeedToTurn;
            this.alignWithElevation = steerToAlign.alignWithElevation;
        }
    }
}
