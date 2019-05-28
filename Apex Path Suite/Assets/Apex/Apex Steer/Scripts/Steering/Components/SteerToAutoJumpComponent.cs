/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using Apex.Services;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// A steering component that allows units to automatically jump onto elevated terrain that is within the height navigation capabilities.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer to Auto Jump", 1034)]
    [ApexComponent("Steering")]
    public class SteerToAutoJumpComponent : SteeringComponent
    {
        /// <summary>
        /// How far ahead the unit will scan for elevated terrain that it needs to jump onto.
        /// </summary>
        [Tooltip("How far ahead the unit will scan for elevated terrain that it needs to jump onto.")]
        public float scanDistance = 1f;

        /// <summary>
        /// The minimum height of elevated terrain to cause the unit to jump.
        /// </summary>
        [Tooltip("The minimum height of elevated terrain to cause the unit to jump.")]
        public float minimumHeightToJump = 0.4f;

        private IUnitFacade _unit;
        private ISampleHeights _heightSampler;
        private float _force;
        private float _targetHeight;
        private bool _jumping;

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (!_jumping)
            {
                if (!_unit.isGrounded)
                {
                    return;
                }

                var scanPoint = _unit.position + (_unit.forward * this.scanDistance);
                var unitElevation = _unit.position.y;
                _targetHeight = _heightSampler.SampleHeight(scanPoint) + _unit.baseToPositionOffset;

                if (_targetHeight - unitElevation < this.minimumHeightToJump || ((_targetHeight - unitElevation) - _unit.heightNavigationCapability.maxClimbHeight) > 0.0001f)
                {
                    return;
                }

                var halfDistance = this.scanDistance / 2f;
                var speed = _unit.velocity.magnitude;
                var timeToTarget = halfDistance / speed;

                //Calculate the distance the unit will drop due to gravity and adjust the target height accordingly
                //Since gravity is assumed negative we do -0.5 instead of just 0.5
                var drop = -0.5f * input.gravity * timeToTarget * timeToTarget;
                _targetHeight += drop;

                _force = _targetHeight / (Time.fixedDeltaTime * timeToTarget);
            }

            _jumping = _unit.position.y < _targetHeight;

            output.overrideHeightNavigation = true;
            output.verticalForce = _force;
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            _heightSampler = GameServices.heightStrategy.heightSampler;

            if (_unit == null)
            {
                _unit = this.GetUnitFacade();
            }

            base.OnStartAndEnable();
        }
    }
}
