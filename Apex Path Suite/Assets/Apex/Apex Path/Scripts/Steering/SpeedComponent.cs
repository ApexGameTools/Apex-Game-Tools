/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// A base component to define the speed settings for a unit.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Basic Speed", 1025)]
    [ApexComponent("Steering")]
    public class SpeedComponent : MonoBehaviour, IDefineSpeed
    {
        [SerializeField, MinCheck(0f, tooltip = "The maximum acceleration rate of the unit (m / s^2), i.e. how fast can the unit reach its desired speed.")]
        private float _maxAcceleration = 20f;

        [SerializeField, MinCheck(0f, tooltip = "The maximum deceleration rate of the unit (m / s^2), i.e. how fast can the unit slow down.")]
        private float _maxDeceleration = 30f;

        [SerializeField, MinCheck(0f, tooltip = "The maximum angular acceleration rate of the unit (rads / s^2), i.e. how fast can the unit reach its desired turn speed.")]
        private float _maxAngularAcceleration = 40f;

        [SerializeField, MinCheck(0f, tooltip = "The maximum turn speed of the unit (rads / s), i.e. how fast can the unit turn.")]
        private float _maxAngularSpeed = 6f;

        [SerializeField, MinCheck(0.2f, tooltip = "The minimum speed ever, regardless of movement form. Any speed below this will mean a stop.")]
        private float _minimumSpeed = 0.2f;

        [SerializeField, MinCheck(0.2f, tooltip = "The maximum speed, regardless of movement form.")]
        private float _maximumSpeed = 10f;

        /// <summary>
        /// The preferred speed of the unit
        /// </summary>
        protected float _preferredSpeed;

        /// <summary>
        /// Gets or sets the minimum speed ever, regardless of movement form. Any speed below this will mean a stop.
        /// </summary>
        /// <value>
        /// The minimum speed.
        /// </value>
        public virtual float minimumSpeed
        {
            get { return _minimumSpeed; }
            set { _minimumSpeed = value; }
        }

        /// <summary>
        /// Gets the maximum speed of the unit.
        /// </summary>
        /// <value>
        /// The maximum speed.
        /// </value>
        public virtual float maximumSpeed
        {
            get { return _maximumSpeed; }
            set { _maximumSpeed = value; }
        }

        /// <summary>
        /// Gets the maximum acceleration (m / s^2), i.e. how fast can the unit reach its desired speed.
        /// </summary>
        /// <value>
        /// The maximum acceleration.
        /// </value>
        public float maxAcceleration
        {
            get { return _maxAcceleration; }
            set { _maxAcceleration = value; }
        }

        /// <summary>
        /// Gets the maximum deceleration (m / s^2), i.e. how fast can the unit slow down.
        /// </summary>
        /// <value>
        /// The maximum deceleration.
        /// </value>
        public float maxDeceleration
        {
            get { return _maxDeceleration; }
            set { _maxDeceleration = value; }
        }

        /// <summary>
        /// Gets the maximum angular acceleration (rads / s^2), i.e. how fast can the unit reach its desired turn speed.
        /// </summary>
        /// <value>
        /// The maximum angular acceleration.
        /// </value>
        public float maxAngularAcceleration
        {
            get { return _maxAngularAcceleration; }
            set { _maxAngularAcceleration = value; }
        }

        /// <summary>
        /// Gets the maximum angular speed (rads / s), i.e. how fast can the unit turn.
        /// </summary>
        /// <value>
        /// The maximum angular speed.
        /// </value>
        public float maximumAngularSpeed
        {
            get { return _maxAngularSpeed; }
            set { _maxAngularSpeed = value; }
        }

        /// <summary>
        /// Sets the preferred speed of the unit.
        /// </summary>
        /// <param name="speed">The speed.</param>
        public virtual void SetPreferredSpeed(float speed)
        {
            _preferredSpeed = Mathf.Min(speed, this.maximumSpeed);
        }

        /// <summary>
        /// Gets the preferred speed of the unit.
        /// </summary>
        /// <param name="currentMovementDirection">The current movement direction.</param>
        /// <returns>
        /// The preferred speed
        /// </returns>
        public virtual float GetPreferredSpeed(Vector3 currentMovementDirection)
        {
            return _preferredSpeed;
        }

        /// <summary>
        /// Signal that the unit has stopped.
        /// </summary>
        public virtual void SignalStop()
        {
            /* Default a NOOP */
        }

        /// <summary>
        /// Clones settings from another speed component.
        /// </summary>
        /// <param name="speedComponent">The speed component to clone from.</param>
        public virtual void CloneFrom(IDefineSpeed speedComponent)
        {
            this.maxAngularAcceleration = speedComponent.maxAngularAcceleration;
            this.maximumAngularSpeed = speedComponent.maximumAngularSpeed;

            this.maxAcceleration = speedComponent.maxAcceleration;
            this.maxDeceleration = speedComponent.maxDeceleration;
            this.minimumSpeed = speedComponent.minimumSpeed;
            this.maximumSpeed = speedComponent.maximumSpeed;

            var sc = speedComponent as SpeedComponent;
            if (sc != null)
            {
                _preferredSpeed = sc._preferredSpeed;
            }
        }
    }
}
