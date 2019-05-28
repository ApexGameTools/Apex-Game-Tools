/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Base class for orientation components
    /// </summary>
    public abstract class OrientationComponent : ExtendedMonoBehaviour, IOrientationBehaviour
    {
        /// <summary>
        /// The priority of this orientation behaviour relative to others. Only the behaviour with the highest priority will influence the orientation of the unit.
        /// </summary>
        [MinCheck(0, tooltip = "The priority of this orientation behaviour relative to others. Only the behaviour with the highest priority will influence the orientation of the unit.")]
        public int priority;

        /// <summary>
        /// The distance within which the unit will start to slow down its rotation speed to smoothly 'arrive' at the designated rotation
        /// </summary>
        //[Range(0f, 1f)]
        [RangeX(0f, 1f, tooltip = "The distance within which the unit will start to slow down its rotation speed to smoothly 'arrive' at the designated rotation")]
        public float slowingDistance = 0.8f;

        /// <summary>
        /// The algorithm used to slow the unit's rotation.
        /// Linear works fine with short slowing distances, but logarithmic shows its worth at longer ones.
        /// </summary>
        [Tooltip("The algorithm used to slow the unit's rotation")]
        public SlowingAlgorithm slowingAlgorithm = SlowingAlgorithm.Logarithmic;

        int IOrientationBehaviour.priority
        {
            get { return this.priority; }
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            var parent = GetComponent<SteerableUnitComponent>();
            if (parent == null)
            {
                Debug.LogError(string.Concat(this.gameObject.name, " : A SteerableUnitComponent is required on all units"));
            }

            parent.RegisterOrientationBehavior(this);
        }

        /// <summary>
        /// Called when disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            var parent = GetComponent<SteerableUnitComponent>();
            if (parent != null)
            {
                parent.UnregisterOrientationBehavior(this);
            }
        }

        /// <summary>
        /// Gets the orientation output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the orientation output.</param>
        /// <param name="output">The orientation output to be populated.</param>
        public abstract void GetOrientation(SteeringInput input, OrientationOutput output);

        /// <summary>
        /// Calculates the angular acceleration.
        /// </summary>
        /// <param name="targetOrientation">The target orientation.</param>
        /// <param name="input">The input.</param>
        /// <returns>The angular acceleration.</returns>
        protected float GetAngularAcceleration(Vector3 targetOrientation, SteeringInput input)
        {
            var dp = Vector3.Dot(input.unit.forward, targetOrientation);

            //Once the alignment is within a reasonable threshold we just zero out the angular velocity
            if (dp > 0.99999f)
            {
                return -input.currentAngularSpeed / Time.fixedDeltaTime;
            }

            var targetSpeed = input.desiredAngularSpeed;

            //Check for slow down. Once the unit reaches the threshold, start slowing down.
            var slowMark = 1 - this.slowingDistance;
            if (dp > this.slowingDistance)
            {
                if (this.slowingAlgorithm == SlowingAlgorithm.Logarithmic)
                {
                    targetSpeed *= Mathf.Log10(((9.0f / slowMark) * (1f - dp)) + 1.0f);
                }
                else
                {
                    targetSpeed *= ((1f - dp) / slowMark);
                }

                //We want to avoid turning too slowly towards the end, so stop at a reasonably slow turn pace.
                //This means that the unit will continue at its current turn rate until alignment is complete.
                if (targetSpeed < 0.1f)
                {
                    return 0f;
                }
            }

            //Scale the angular acceleration remaining to reach desired velocity as quick as possible within the bounds of max acceleration.
            float targetAcceleration = input.maxAngularAcceleration;

            targetSpeed = (targetSpeed - input.currentAngularSpeed) / Time.fixedDeltaTime;

            return Mathf.Clamp(targetSpeed, -targetAcceleration, targetAcceleration);
        }
    }
}
