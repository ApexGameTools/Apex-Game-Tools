/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// Aligns group members with their model unit, effectively forcing all to face the same direction when the group is not moving.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer to Align With Model Unit", 1032)]
    [ApexComponent("Steering")]
    public class SteerToAlignWithModelUnit : OrientationComponent
    {
        /// <summary>
        /// Gets the orientation output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the orientation output.</param>
        /// <param name="output">The orientation output to be populated.</param>
        public override void GetOrientation(SteeringInput input, OrientationOutput output)
        {
            if (input.currentPlanarVelocity.sqrMagnitude > 0f)
            {
                return;
            }

            var grp = input.unit.transientGroup;
            if (grp == null)
            {
                return;
            }

            var model = grp.modelUnit;
            if (model == null)
            {
                return;
            }

            var targetOrientation = model.forward;
            output.desiredOrientation = targetOrientation;
            output.desiredAngularAcceleration = GetAngularAcceleration(targetOrientation, input);
        }
    }
}
