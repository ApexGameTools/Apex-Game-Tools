/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Input for steering components
    /// </summary>
    public sealed class SteeringInput
    {
        /// <summary>
        /// The gravity
        /// </summary>
        public float gravity;

        /// <summary>
        /// The maximum acceleration
        /// </summary>
        public float maxAcceleration;

        /// <summary>
        /// The maximum deceleration
        /// </summary>
        public float maxDeceleration;

        /// <summary>
        /// The maximum angular acceleration
        /// </summary>
        public float maxAngularAcceleration;

        /// <summary>
        /// The current planar velocity
        /// </summary>
        public Vector3 currentPlanarVelocity;

        /// <summary>
        /// The current spatial velocity
        /// </summary>
        public Vector3 currentSpatialVelocity;

        /// <summary>
        /// The current full velocity
        /// </summary>
        public Vector3 currentFullVelocity;

        /// <summary>
        /// The current angular speed
        /// </summary>
        public float currentAngularSpeed;

        /// <summary>
        /// The desired speed
        /// </summary>
        public float desiredSpeed;

        /// <summary>
        /// The desired angular speed
        /// </summary>
        public float desiredAngularSpeed;

        /// <summary>
        /// The grid
        /// </summary>
        public IGrid grid;

        /// <summary>
        /// The unit
        /// </summary>
        public IUnitFacade unit;

        /// <summary>
        /// The time frame for the operation
        /// </summary>
        public float deltaTime;
    }
}
