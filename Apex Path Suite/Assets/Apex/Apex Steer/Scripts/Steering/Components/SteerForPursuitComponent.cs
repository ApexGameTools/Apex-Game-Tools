/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to pursue a given moving target.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Pursuit", 1027)]
    [ApexComponent("Steering")]
    public class SteerForPursuitComponent : SteeringComponent
    {
        /// <summary>
        /// Sets the target for pursuit.
        /// </summary>
        [Tooltip("The target used for pursuit, i.e. the target that this unit will attempt to pursue")]
        public Transform target;

        /// <summary>
        /// The radius within which the pursuing unit may start pursuing towards the target
        /// </summary>
        [Tooltip("The radius within which the pursuing unit may start pursuing the target (set to 0 to always pursue).")]
        public float awarenessRadius = 10f;

        /// <summary>
        /// The radius at which the unit will slow to a stop and move no further towards the target
        /// </summary>
        [Tooltip("The radius at which the unit will slow to a stop and move no further towards the target.")]
        public float stopRadius = 1.5f;

        /// <summary>
        /// The time over which to stop as permitted by deceleration capabilities.
        /// </summary>
        [Tooltip("The time over which to stop as permitted by deceleration capabilities.")]
        public float stopTimeFrame = 0.1f;

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
            Vector3 unitPos = input.unit.position;
            var squaredDistance = (targetPos - unitPos).sqrMagnitude;
            if (this.awarenessRadius > 0f && squaredDistance > (this.awarenessRadius * this.awarenessRadius))
            {
                // if target is outside awareness radius
                return;
            }

            if (squaredDistance <= (this.stopRadius * this.stopRadius))
            {
                // if inside stop radius, then start arriving
                output.desiredAcceleration = Arrive(this.stopTimeFrame, input);
                return;
            }

            Vector3 pursuePos = Vector3.zero;
            var targetUnit = target.GetUnitFacade(false);

            if (targetUnit == null)
            {
                // if target has no unit facade, then it is probably not a moving target anyway
                pursuePos = targetPos;
            }
            else
            {
                var distToOther = (targetPos - unitPos).magnitude;
                var targetSpeed = targetUnit.velocity.magnitude;

                var predictionTime = 0.1f;
                if (targetSpeed > 0f)
                {
                    //Half the prediction time for better behavior
                    predictionTime = (distToOther / targetSpeed) / 2f;
                }

                pursuePos = targetPos + (targetUnit.velocity * predictionTime);
            }

            if (pursuePos.sqrMagnitude == 0f)
            {
                return;
            }

            // seek the pursue position and allow speed-up
            output.desiredAcceleration = Seek(pursuePos, input);
            output.maxAllowedSpeed = input.unit.maximumSpeed;
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