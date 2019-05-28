/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Base class for steering components, that is components that steer the unit in some direction at some speed according to some logic.
    /// </summary>
    public abstract class SteeringComponent : ExtendedMonoBehaviour, ISteeringBehaviour
    {
        /// <summary>
        /// The weight this component's input will have in relation to other steering components.
        /// </summary>
        [Tooltip("The weight this component's input will have in relation to other steering components.")]
        public float weight = 1.0f;

        /// <summary>
        /// The priority of this steering behaviour relative to others. Only behaviours with the highest priority will influence the steering of the unit, provided they have any steering output.
        /// </summary>
        [MinCheck(0, tooltip = "The priority of this steering behaviour relative to others. Only behaviours with the highest priority will influence the steering of the unit, provided they have any steering output")]
        public int priority;

        int ISteeringBehaviour.priority
        {
            get { return this.priority; }
        }

        /// <summary>
        /// Stop the unit.
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// Gets the steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public void GetSteering(SteeringInput input, SteeringOutput output)
        {
            GetDesiredSteering(input, output);

            if (output.hasOutput)
            {
                output.desiredAcceleration *= this.weight;
            }
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public abstract void GetDesiredSteering(SteeringInput input, SteeringOutput output);

        /// <summary>
        /// Awakes this instance.
        /// </summary>
        protected virtual void Awake()
        {
            this.WarnIfMultipleInstances();
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            var parent = GetComponent<SteerableUnitComponent>();
            parent.RegisterSteeringBehavior(this);
        }

        /// <summary>
        /// Called when disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            var parent = GetComponent<SteerableUnitComponent>();
            parent.UnregisterSteeringBehavior(this);
        }

        /// <summary>
        /// Seeks the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="input">The input.</param>
        /// <returns>The seek acceleration vector</returns>
        protected Vector3 Seek(Vector3 destination, SteeringInput input)
        {
            return Seek(input.unit.position, destination, input);
        }

        /// <summary>
        /// Seeks the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="input">The input.</param>
        /// <param name="maxAcceleration">The maximum allowed acceleration.</param>
        /// <returns>The seek acceleration vector</returns>
        protected Vector3 Seek(Vector3 destination, SteeringInput input, float maxAcceleration)
        {
            return Seek(input.unit.position, destination, input, maxAcceleration);
        }

        /// <summary>
        /// Seeks from the specified position to a destination.
        /// </summary>
        /// <param name="position">The position from which to seek.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="input">The input.</param>
        /// <returns>The seek acceleration vector</returns>
        protected Vector3 Seek(Vector3 position, Vector3 destination, SteeringInput input)
        {
            return Seek(position, destination, input, input.maxAcceleration);
        }

        /// <summary>
        /// Seeks from the specified position to a destination.
        /// </summary>
        /// <param name="position">The position from which to seek.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="input">The input.</param>
        /// <param name="maxAcceleration">The maximum allowed acceleration.</param>
        /// <returns>The seek acceleration vector</returns>
        protected Vector3 Seek(Vector3 position, Vector3 destination, SteeringInput input, float maxAcceleration)
        {
            var dir = position.DirToXZ(destination);
            var desiredVelocity = dir.normalized * input.desiredSpeed;

            return Vector3.ClampMagnitude((desiredVelocity - input.currentPlanarVelocity) / input.deltaTime, maxAcceleration);
        }

        /// <summary>
        /// Flees the specified 'from' position.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="input">The input.</param>
        /// <returns>The flee acceleration vector</returns>
        protected Vector3 Flee(Vector3 from, SteeringInput input)
        {
            return Seek(from, input.unit.transform.position, input);
        }

        /// <summary>
        /// Flees the specified 'from' position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="from">From.</param>
        /// <param name="input">The input.</param>
        /// <returns>The flee acceleration vector</returns>
        protected Vector3 Flee(Vector3 position, Vector3 from, SteeringInput input)
        {
            return Seek(from, position, input);
        }

        /// <summary>
        /// Arrives, i.e. stops as fast as possible.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The deceleration vector</returns>
        protected Vector3 Arrive(SteeringInput input)
        {
            return Vector3.ClampMagnitude(-input.currentPlanarVelocity / Time.fixedDeltaTime, input.maxDeceleration);
        }

        /// <summary>
        /// Arrives over a certain time.
        /// </summary>
        /// <param name="timeToTarget">The time to target, i.e. the time over which to stop.</param>
        /// <param name="input">The input.</param>
        /// <returns>The deceleration vector</returns>
        protected Vector3 Arrive(float timeToTarget, SteeringInput input)
        {
            timeToTarget = Mathf.Max(timeToTarget, Time.fixedDeltaTime);
            return Vector3.ClampMagnitude(-input.currentPlanarVelocity / timeToTarget, input.maxDeceleration);
        }
    }
}