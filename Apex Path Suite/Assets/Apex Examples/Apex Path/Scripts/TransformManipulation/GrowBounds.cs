/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.TransformManipulation
{
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/GrowBounds", 1008)]
    public class GrowBounds : MonoBehaviour
    {
        public Vector3 growthVector;
        public float growthSpeed;
        public float maxGrowth;

        private float _currentGrowth = 0;
        private Transform _transform;
        private Vector3 _originalScale;

        private void Start()
        {
            _transform = transform;
            _originalScale = _transform.localScale;
        }

        private void Update()
        {
            if ((_currentGrowth > maxGrowth && growthSpeed > 0f) || (_currentGrowth < 0f && growthSpeed < 0f))
            {
                growthSpeed *= -1;
            }
            
            _currentGrowth += growthSpeed * Time.deltaTime;

            _transform.localScale = _originalScale + (this.growthVector * _currentGrowth);
        }
    }
}
