/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using UnityEngine;

    /// <summary>
    /// Output for orientation steering.
    /// </summary>
    public sealed class OrientationOutput
    {
        /// <summary>
        /// The desired orientation
        /// </summary>
        public Vector3 desiredOrientation;

        private float _desiredAngularAcceleration;
        private bool _hasOutput;

        /// <summary>
        /// Gets or sets the desired angular acceleration.
        /// </summary>
        /// <value>
        /// The desired angular acceleration.
        /// </value>
        public float desiredAngularAcceleration
        {
            get
            {
                return _desiredAngularAcceleration;
            }

            set
            {
                _desiredAngularAcceleration = value;
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
            this.desiredOrientation = Vector3.zero;
            this.desiredAngularAcceleration = 0f;
            _hasOutput = false;
        }
    }
}
