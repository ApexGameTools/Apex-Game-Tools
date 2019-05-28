/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to seek towards a given target.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Seek", 1028)]
    [ApexComponent("Steering")]
    public class SteerForSeekComponent : ArrivalBase
    {
        /// <summary>
        /// Sets the target for seeking.
        /// </summary>
        [Tooltip("The target used for seeking")]
        public Transform target;

        /// <summary>
        /// The radius within which the unit will start seeking the target (set to 0 to always seek target)
        /// </summary>
        [Tooltip("The radius within which the seeking unit may start seeking the target (set to 0 to always seek).")]
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
            // if target is within awareness radius - or awareness radius has been set to 0 or below, then seek
            if ((targetPos - input.unit.position).sqrMagnitude < (this.awarenessRadius * this.awarenessRadius) || this.awarenessRadius <= 0f)
            {
                // arrive at the target position
                output.desiredAcceleration = Arrive(targetPos, input);
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
