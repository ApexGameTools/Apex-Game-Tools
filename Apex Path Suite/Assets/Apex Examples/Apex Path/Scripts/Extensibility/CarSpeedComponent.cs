/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.Steering;
    using UnityEngine;

    /// <summary>
    /// A component to define the speed settings for a humanoid unit.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Extensibility/Car Speed", 1001)]
    public class CarSpeedComponent : SpeedComponent
    {
        /// <summary>
        /// The strafe maximum speed, that is the highest speed possible when moving side ways
        /// </summary>
        public float sidewaysSpeedFactor = 0.1f;

        /// <summary>
        /// The back pedal maximum speed, that is the highest speed possible when moving backwards
        /// </summary>
        public float reverseMaxSpeed = 3.0f;

        private Transform _transform;

        private void Awake()
        {
            _transform = this.transform;

            _preferredSpeed = this.maximumSpeed;
        }

        /// <summary>
        /// Gets the preferred speed of the unit.
        /// </summary>
        /// <param name="currentMovementDirection">The current movement direction.</param>
        /// <returns>
        /// The preferred speed
        /// </returns>
        public override float GetPreferredSpeed(Vector3 currentMovementDirection)
        {
            var dp = Vector3.Dot(currentMovementDirection, _transform.forward);

            if (dp < 0.7f)
            {
                //Between 60 and 120 degrees movement is considered a strafe
                if (dp > -0.7f)
                {
                    return _preferredSpeed * this.sidewaysSpeedFactor;
                }

                //Beyond 120 degrees its a backpedal.
                if (_preferredSpeed > this.reverseMaxSpeed)
                {
                    return this.reverseMaxSpeed;
                }
            }

            return _preferredSpeed;
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="speedComponent">The component to clone from</param>
        public override void CloneFrom(IDefineSpeed speedComponent)
        {
            base.CloneFrom(speedComponent);

            var hs = speedComponent as CarSpeedComponent;
            if (hs != null)
            {
                this.sidewaysSpeedFactor = hs.sidewaysSpeedFactor;
                this.reverseMaxSpeed = hs.reverseMaxSpeed;
            }
        }
    }
}
