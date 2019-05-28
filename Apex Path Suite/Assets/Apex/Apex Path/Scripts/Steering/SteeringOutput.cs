/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using UnityEngine;

    /// <summary>
    /// Output for locomotion steering
    /// </summary>
    public sealed class SteeringOutput
    {
        private bool _hasOutput;
        private bool _pause;
        private Vector3 _desiredAcceleration;
        private float _verticalForce;

        /// <summary>
        /// The maximum allowed speed
        /// </summary>
        public float maxAllowedSpeed;

        /// <summary>
        /// Whether to override height navigation for the unit. This is typically something you would set to true if supplying a <see cref="verticalForce"/>
        /// </summary>
        public bool overrideHeightNavigation;

        /// <summary>
        /// Gets or sets the desired acceleration.
        /// </summary>
        /// <value>
        /// The desired acceleration.
        /// </value>
        public Vector3 desiredAcceleration
        {
            get
            {
                return _desiredAcceleration;
            }

            set
            {
                _desiredAcceleration = value;
                _hasOutput = true;
            }
        }

        /// <summary>
        /// Gets or sets the vertical force.
        /// </summary>
        /// <value>
        /// The vertical force.
        /// </value>
        public float verticalForce
        {
            get
            {
                return _verticalForce;
            }

            set
            {
                _verticalForce = value;
                _hasOutput = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to pause steering.
        /// </summary>
        /// <value>
        ///   <c>true</c> to pause; otherwise, <c>false</c>.
        /// </value>
        public bool pause
        {
            get
            {
                return _pause;
            }

            set
            {
                _pause = value;
                _hasOutput = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has output.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has output; otherwise, <c>false</c>.
        /// </value>
        public bool hasOutput
        {
            get { return _hasOutput; }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _desiredAcceleration = Vector3.zero;
            _verticalForce = 0f;
            _hasOutput = false;
            _pause = false;
            this.overrideHeightNavigation = false;
            this.maxAllowedSpeed = 0f;
        }
    }
}
