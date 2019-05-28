/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// A simple SteeringComponent for making units jump on request.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer to Jump", 1036)]
    [ApexComponent("Steering")]
    public class SteerToJumpComponent : SteeringComponent
    {
        /// <summary>
        /// The default height that jumping reaches.
        /// </summary>
        [Tooltip("The default height that jumping reaches.")]
        public float defaultHeight = 3f;

        /// <summary>
        /// The default force power used for jumping.
        /// </summary>
        [Tooltip("The default force power used for jumping.")]
        public float defaultForce = 200f;

        private IUnitFacade _unit;
        private float _force;
        private float _targetHeight;
        private bool _jumping;

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            base.OnStartAndEnable();
            _unit = this.GetUnitFacade();
        }

        /// <summary>
        /// Makes this unit jump to default height with default force.
        /// </summary>
        public void Jump()
        {
            Jump(this.defaultHeight, this.defaultForce);
        }

        /// <summary>
        /// Makes this unit jump to the specified height.
        /// </summary>
        /// <param name="toHeight">Jump height.</param>
        /// <param name="force">Jump force power.</param>
        public void Jump(float toHeight, float force)
        {
            if (_jumping || !_unit.isGrounded)
            {
                // if already in a jump, don't start a new jump
                return;
            }

            _jumping = true;
            _force = force;
            _targetHeight = _unit.position.y + toHeight;
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (!_jumping)
            {
                // exit early if not jumping
                return;
            }

            // start jumping if unit is below the target height
            _jumping = _unit.position.y < _targetHeight;

            output.overrideHeightNavigation = true;
            output.verticalForce = _force;
        }
    }
}