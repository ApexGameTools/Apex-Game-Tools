/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.TransformManipulation
{
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Rotate", 1021)]
    public class Rotate : MonoBehaviour
    {
        public float speedX;
        public float speedY;
        public float speedZ;

        private Transform _transform;

        private void Start()
        {
            _transform = transform;
        }

        private void Update()
        {
            var rot = Vector3.zero;
            if (speedX > 0f)
            {
                rot.x = (speedX * Time.deltaTime);
            }

            if (speedY > 0f)
            {
                rot.y = (speedY * Time.deltaTime);
            }

            if (speedZ > 0f)
            {
                rot.z = (speedZ * Time.deltaTime);
            }

            _transform.Rotate(rot);
        }
    }
}
