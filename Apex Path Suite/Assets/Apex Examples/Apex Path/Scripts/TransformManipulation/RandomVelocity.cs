/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.TransformManipulation
{
    using UnityEngine;

    /// <summary>
    /// Moves a transform between two points.
    /// </summary>
    [AddComponentMenu("Apex/Examples/RandomVelocity", 1020)]
    [RequireComponent(typeof(Rigidbody))]
    public class RandomVelocity : MonoBehaviour
    {
        /// <summary>
        /// The speed
        /// </summary>
        public float speed = 3;

        /// <summary>
        /// The tolerance (when to switch directions)
        /// </summary>
        public float changeInterval = 10f;

        private Vector3 _direction;
        private Rigidbody _rigidbody;
        private float _nextDirectionAt;

        private void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Time.time > _nextDirectionAt)
            {
                _direction = Random.insideUnitSphere.AdjustAxis(0f, WorldGeometry.Axis.Y).normalized;
                _nextDirectionAt = Time.time + this.changeInterval;
            }

            _rigidbody.velocity = _direction * speed;
        }
    }
}
