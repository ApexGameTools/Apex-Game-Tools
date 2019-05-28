/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to evade a given moving target.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Evasion", 1023)]
    [ApexComponent("Steering")]
    public class SteerForEvasionComponent : SteeringComponent
    {
        /// <summary>
        /// Sets the target for evasion.
        /// </summary>
        [Tooltip("The target for evasion, i.e. the target that this unit will attempt to evade.")]
        public Transform target = null;

        /// <summary>
        /// The radius within which the evading unit may start evading its target.
        /// </summary>
        [Tooltip("The radius within which the evading unit may start evading its target (set to 0 to always evade from target).")]
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
                // if no target, exit early
                return;
            }

            Vector3 targetPos = this.target.position;
            Vector3 unitPos = input.unit.position;
            if (this.awarenessRadius > 0f && (targetPos - unitPos).sqrMagnitude > (this.awarenessRadius * this.awarenessRadius))
            {
                // if distance is more than awareness radius, then exit
                return;
            }

            Vector3 evadePos = Vector3.zero;
            var targetUnit = this.target.GetUnitFacade();

            if (targetUnit == null)
            {
                // if target has no unit facade, then it is probably not a moving target anyway
                evadePos = targetPos;
            }
            else
            {
                var distToOther = (targetPos - unitPos).magnitude;
                var otherSpeed = targetUnit.velocity.magnitude;

                var predictionTime = 0.1f;
                if (otherSpeed > 0f)
                {
                    //Half the prediction time for better behavior
                    predictionTime = (distToOther / otherSpeed) / 2f;
                }

                evadePos = targetPos + (targetUnit.velocity * predictionTime);
            }

            if (evadePos.sqrMagnitude == 0f)
            {
                return;
            }

            // flee evade position and allow speed-up
            output.desiredAcceleration = Flee(evadePos, input);
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