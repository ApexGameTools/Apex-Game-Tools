/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// Orients the unit to look in the <see cref="Units.IUnitFacade.lookAt"/> direction if set.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer to Look At", 1036)]
    [ApexComponent("Steering")]
    public class SteerToLookAt : OrientationComponent
    {
        /// <summary>
        /// Gets the orientation.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public override void GetOrientation(SteeringInput input, OrientationOutput output)
        {
            if (!input.unit.lookAt.HasValue)
            {
                return;
            }

            var targetOrientation = (input.unit.lookAt.Value - input.unit.position).OnlyXZ();
            output.desiredOrientation = targetOrientation;
            output.desiredAngularAcceleration = GetAngularAcceleration(targetOrientation.normalized, input);
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="steerToLook">The component to clone from.</param>
        public void CloneFrom(SteerToLookAt steerToLook)
        {
            this.priority = steerToLook.priority;
            this.slowingDistance = steerToLook.slowingDistance;
            this.slowingAlgorithm = steerToLook.slowingAlgorithm;
        }
    }
}
