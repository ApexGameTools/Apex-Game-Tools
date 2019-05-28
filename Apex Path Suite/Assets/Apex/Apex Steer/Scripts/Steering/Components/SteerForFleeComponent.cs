/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to flee away from a given target.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Flee", 1024)]
    [ApexComponent("Steering")]
    public class SteerForFleeComponent : SteeringComponent
    {
        /// <summary>
        /// Sets the target for fleeing.
        /// </summary>
        [Tooltip("The target used for fleeing, i.e. the target that this unit will attempt to flee.")]
        public Transform target;

        /// <summary>
        /// The radius within which the fleeing unit may start fleeing away from the target (set to 0 to always flee from target)
        /// </summary>
        [Tooltip("The radius within which the fleeing unit may start fleeing away from the target (set to 0 to always flee from target)")]
        public float awarenessRadius = 10f;

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (this.target == null)
            {
                return;
            }

            Vector3 targetPos = this.target.position;
            // if target is within awareness radius - or awareness radius has been set to 0 or below, then flee
            if ((targetPos - input.unit.position).sqrMagnitude < (this.awarenessRadius * this.awarenessRadius) || this.awarenessRadius <= 0f)
            {
                // flee and allow speed-up
                output.desiredAcceleration = Flee(targetPos, input);
                output.maxAllowedSpeed = input.unit.maximumSpeed;
            }            
        }

        /// <summary>
        /// Stop the unit.
        /// </summary>
        public override void Stop()
        {
            this.target = null;
        }
    }
}