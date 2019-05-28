/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Base for arrival based steering
    /// </summary>
    public abstract class ArrivalBase : SteeringComponent
    {
        /// <summary>
        /// The distance within which the unit will start to slow down for arrival.
        /// </summary>
        [MinCheck(0f, tooltip = "The distance within which the unit will start to slow down for arrival.")]
        public float slowingDistance = 1.5f;

        /// <summary>
        /// Determines whether the slowing distance is automatically calculated based on the unit's speed and deceleration capabilities.
        /// </summary>
        [Tooltip("Determines whether the slowing distance is automatically calculated based on the unit's speed and deceleration capabilities.")]
        public bool autoCalculateSlowingDistance = true;

        /// <summary>
        /// The distance from the final destination where the unit will stop
        /// </summary>
        [MinCheck(0.1f, tooltip = "The distance from the final destination where the unit will stop.")]
        public float arrivalDistance = 0.2f;

        /// <summary>
        /// The algorithm used to slow the unit for arrival.
        /// Linear works fine with short slowing distances, but logarithmic shows its worth at longer ones.
        /// </summary>
        [Tooltip("The algorithm used to slow the unit for arrival.")]
        public SlowingAlgorithm slowingAlgorithm = SlowingAlgorithm.Logarithmic;

        /// <summary>
        /// Gets a value indicating whether this instance has arrived.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has arrived; otherwise, <c>false</c>.
        /// </value>
        protected bool hasArrived
        {
            get;
            private set;
        }

        /// <summary>
        /// Awakes this instance.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (this.autoCalculateSlowingDistance)
            {
                this.slowingDistance = 0f;
            }
        }

        /// <summary>
        /// Calculates the arrive acceleration vector for the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="input">The input.</param>
        /// <returns>The acceleration vector</returns>
        protected Vector3 Arrive(Vector3 destination, SteeringInput input)
        {
            return Arrive(destination, input, input.desiredSpeed);
        }

        /// <summary>
        /// Calculates the arrive acceleration vector for the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="input">The input.</param>
        /// <param name="desiredSpeed">The desired speed.</param>
        /// <returns>The acceleration vector</returns>
        protected Vector3 Arrive(Vector3 destination, SteeringInput input, float desiredSpeed)
        {
            var dir = input.unit.position.DirToXZ(destination);

            var distance = dir.magnitude;
            var arriveDistance = distance - this.arrivalDistance;

            //Arrival is accurate within 10 centimeters
            this.hasArrived = arriveDistance <= 0.01f;
            if (this.hasArrived)
            {
                if (this.autoCalculateSlowingDistance)
                {
                    this.slowingDistance = 0f;
                }

                return -input.currentPlanarVelocity / Time.fixedDeltaTime;
            }

            //Calculate slowing distance if applicable
            if (this.autoCalculateSlowingDistance)
            {
                CalculateRequiredSlowingDistance(input);
            }

            //Find the target speed for arrival
            var targetSpeed = desiredSpeed;

            if (arriveDistance < this.slowingDistance)
            {
                if (this.slowingAlgorithm == SlowingAlgorithm.Logarithmic)
                {
                    targetSpeed *= Mathf.Log10(((9.0f / this.slowingDistance) * arriveDistance) + 1.0f);
                }
                else
                {
                    targetSpeed *= (arriveDistance / this.slowingDistance);
                }
            }

            var desiredVelocity = (dir / distance) * Mathf.Max(targetSpeed, input.unit.minimumSpeed);

            //Before determining the delta we need to evaluate if we are on arrival
            float targetAcceleration;
            if (desiredVelocity.sqrMagnitude < input.currentPlanarVelocity.sqrMagnitude)
            {
                targetAcceleration = input.maxDeceleration;
            }
            else
            {
                targetAcceleration = input.maxAcceleration;
            }

            desiredVelocity = (desiredVelocity - input.currentPlanarVelocity) / Time.fixedDeltaTime;

            return Vector3.ClampMagnitude(desiredVelocity, targetAcceleration);
        }

        /// <summary>
        /// Calculates the arrive acceleration vector for the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="remainingDistance">The remaining distance.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns>The acceleration vector</returns>
        protected bool Arrive(Vector3 destination, float remainingDistance, SteeringInput input, SteeringOutput output)
        {
            var arriveDistance = remainingDistance - this.arrivalDistance;

            this.hasArrived = arriveDistance <= 0.01f;
            if (this.hasArrived)
            {
                return true;
            }

            var currentVelocity = input.currentPlanarVelocity;
            this.slowingDistance = currentVelocity.sqrMagnitude / (2 * input.maxDeceleration);

            //We want to start stopping when the deceleration capabilities dictate it, so applying v² = u² + 2a * d
            //Since v is 0 (target velocity) and a is negative (deceleration) the acceptable current velocity u can be expressed as u = sqrt(2a * d).
            var maxSpeed = Mathf.Sqrt(2 * input.maxDeceleration * arriveDistance);
            var desiredSpeed = Mathf.Min(maxSpeed, input.desiredSpeed);

            var dir = input.unit.position.DirToXZ(destination);
            var desiredVelocity = dir.normalized * desiredSpeed;

            //While in theory we want to clamp to max deceleration when decelerating, in practice we just want to decelerate by whatever is needed since the deceleration capabilities are already considered above.
            //So this ensures that floating point inaccuracies do not cause imprecise stopping.
            if (desiredSpeed < input.desiredSpeed)
            {
                output.desiredAcceleration = (desiredVelocity - input.currentPlanarVelocity) / input.deltaTime;
            }
            else
            {
                output.desiredAcceleration = Vector3.ClampMagnitude((desiredVelocity - input.currentPlanarVelocity) / input.deltaTime, input.maxAcceleration);
            }

            return false;
        }

        /// <summary>
        /// Calculates the required slowing distance.
        /// </summary>
        /// <param name="input">The input.</param>
        protected void CalculateRequiredSlowingDistance(SteeringInput input)
        {
            float currentSpeed = input.currentPlanarVelocity.magnitude;
            if (this.slowingAlgorithm == SlowingAlgorithm.Logarithmic)
            {
                this.slowingDistance = Mathf.Max(this.slowingDistance, Mathf.Ceil(((currentSpeed * currentSpeed) / (2 * input.maxDeceleration)) * 1.1f) + 1f);
            }
            else
            {
                this.slowingDistance = Mathf.Max(this.slowingDistance, (currentSpeed * currentSpeed) / (2 * input.maxDeceleration));
            }
        }
    }
}