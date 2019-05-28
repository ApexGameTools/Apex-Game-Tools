/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using UnityEngine;

    /// <summary>
    /// A vector that represents a direction, e.g. (1,0,0) is right, (1,1,1) is up forward right etc.
    /// </summary>
    public struct DirectionVector
    {
        private float _x;
        private float _y;
        private float _z;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionVector"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public DirectionVector(float x, float y, float z)
        {
            _x = Clamp(x);
            _y = Clamp(y);
            _z = Clamp(z);
        }

        /// <summary>
        /// Gets the left vector.
        /// </summary>
        public static DirectionVector left
        {
            get { return new DirectionVector(-1f, 0f, 0f); }
        }

        /// <summary>
        /// Gets the right vector.
        /// </summary>
        public static DirectionVector right
        {
            get { return new DirectionVector(1f, 0f, 0f); }
        }

        /// <summary>
        /// Gets the foward vector.
        /// </summary>
        public static DirectionVector forward
        {
            get { return new DirectionVector(0f, 0f, 1f); }
        }

        /// <summary>
        /// Gets the back vector.
        /// </summary>
        public static DirectionVector back
        {
            get { return new DirectionVector(0f, 0f, -1f); }
        }

        /// <summary>
        /// Gets the up vector.
        /// </summary>
        public static DirectionVector up
        {
            get { return new DirectionVector(0f, 1f, 0f); }
        }

        /// <summary>
        /// Gets the down vector.
        /// </summary>
        public static DirectionVector down
        {
            get { return new DirectionVector(0f, -1f, 0f); }
        }

        /// <summary>
        /// Gets the x-value.
        /// </summary>
        public float x
        {
            get { return _x; }
        }

        /// <summary>
        /// Gets the y-value.
        /// </summary>
        public float y
        {
            get { return _y; }
        }

        /// <summary>
        /// Gets the z-value.
        /// </summary>
        public float z
        {
            get { return _z; }
        }

        /// <summary>
        /// Implements implicit conversion operator from Vector3
        /// </summary>
        /// <param name="vector">The <see cref="DirectionVector"/> to convert to a Vector3</param>
        /// <returns></returns>
        public static implicit operator Vector3(DirectionVector vector)
        {
            return new Vector3(vector._x, vector._y, vector._z);
        }

        /// <summary>
        /// Implements the implicit conversion operator to Vector3
        /// </summary>
        /// <param name="vector">The Vector3 to convert the <see cref="DirectionVector"/> from</param>
        /// <returns></returns>
        public static implicit operator DirectionVector(Vector3 vector)
        {
            return new DirectionVector(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The first <see cref="DirectionVector"/>.</param>
        /// <param name="b">The second <see cref="DirectionVector"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static DirectionVector operator +(DirectionVector a, DirectionVector b)
        {
            return new DirectionVector(a._x + b._x, a._y + b._y, a._z + b._z);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first <see cref="DirectionVector"/>.</param>
        /// <param name="b">The second <see cref="DirectionVector"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static DirectionVector operator -(DirectionVector a, DirectionVector b)
        {
            return new DirectionVector(a._x - b._x, a._y - b._y, a._z - b._z);
        }

        /// <summary>
        /// Implements the operator *. This multiplies each axis of the Vector3 by it's corresponding direction.
        /// </summary>
        /// <param name="a">The first <see cref="Vector3"/>.</param>
        /// <param name="b">The second <see cref="DirectionVector"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Vector3 operator *(Vector3 a, DirectionVector b)
        {
            return new Vector3(a.x * b._x, a.y * b._y, a.z * b.z);
        }

        private static float Clamp(float f)
        {
            if (f == 0f)
            {
                return f;
            }

            if (f > 0f)
            {
                return 1f;
            }

            return -1f;
        }
    }
}
