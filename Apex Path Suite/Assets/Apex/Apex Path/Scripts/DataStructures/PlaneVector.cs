/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.DataStructures
{
    using UnityEngine;

    /// <summary>
    /// Represents a vector of only x and z
    /// </summary>
    public struct PlaneVector
    {
        /// <summary>
        /// The x coordinate
        /// </summary>
        public float x;

        /// <summary>
        /// The z coordinate
        /// </summary>
        public float z;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaneVector"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public PlaneVector(float x, float z)
        {
            this.x = x;
            this.z = z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaneVector"/> struct.
        /// </summary>
        /// <param name="vector">The Vector3 to copy x and z coordinates from.</param>
        public PlaneVector(Vector3 vector)
        {
            this.x = vector.x;
            this.z = vector.z;
        }

        /// <summary>
        /// Returns a <see cref="PlaneVector"/> with 0 as both its components.
        /// </summary>
        public static PlaneVector zero
        {
            get
            {
                return new PlaneVector(0f, 0f);
            }
        }

        /// <summary>
        /// Returns the x,z magnitude (use sqrMagnitude when possible)
        /// </summary>
        public float magnitude
        {
            get
            {
                return PlaneVector.Magnitude(this);
            }
        }

        /// <summary>
        /// Returns the squared x,z magnitude (performs better than magnitude)
        /// </summary>
        public float sqrMagnitude
        {
            get
            {
                return (this.x * this.x) + (this.z * this.z);
            }
        }

        /// <summary>
        /// Returns the <see cref="PlaneVector"/> normalized, meaning that it has been divided by its own magnitude.
        /// </summary>
        public PlaneVector normalized
        {
            get
            {
                return PlaneVector.Normalize(this);
            }
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The first <see cref="PlaneVector"/>.</param>
        /// <param name="b">The second <see cref="PlaneVector"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static PlaneVector operator +(PlaneVector a, PlaneVector b)
        {
            return new PlaneVector(a.x + b.x, a.z + b.z);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first <see cref="PlaneVector"/>.</param>
        /// <param name="b">The second <see cref="PlaneVector"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static PlaneVector operator -(PlaneVector a, PlaneVector b)
        {
            return new PlaneVector(a.x - b.x, a.z - b.z);
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">The <see cref="PlaneVector"/> to divide.</param>
        /// <param name="d">The factor with which the plane vector is divided.</param>
        /// <returns></returns>
        public static PlaneVector operator /(PlaneVector a, float d)
        {
            return new PlaneVector(a.x / d, a.z / d);
        }

        /// <summary>
        /// Implements implicit conversion operator from Vector3
        /// </summary>
        /// <param name="vector">The <see cref="PlaneVector"/> to convert to a Vector3</param>
        /// <returns></returns>
        public static implicit operator Vector3(PlaneVector vector)
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        /// <summary>
        /// Implements the implicit conversion operator to Vector3
        /// </summary>
        /// <param name="vector">The Vector3 to convert the <see cref="PlaneVector"/> from</param>
        /// <returns></returns>
        public static implicit operator PlaneVector(Vector3 vector)
        {
            return new PlaneVector(vector.x, vector.z);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(PlaneVector lhs, PlaneVector rhs)
        {
            return PlaneVector.SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(PlaneVector lhs, PlaneVector rhs)
        {
            return PlaneVector.SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
        }

        /// <summary>
        /// Returns a normalized <see cref="PlaneVector"/> (divided by its Magnitude)
        /// </summary>
        /// <param name="value">The <see cref="PlaneVector"/> to normalize.</param>
        /// <returns></returns>
        public static PlaneVector Normalize(PlaneVector value)
        {
            float single = PlaneVector.Magnitude(value);
            if (single <= 1E-05f)
            {
                return PlaneVector.zero;
            }

            return value / single;
        }

        /// <summary>
        /// Returns the magnitude of a <see cref="PlaneVector"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float Magnitude(PlaneVector a)
        {
            return Mathf.Sqrt((a.x * a.x) + (a.z * a.z));
        }

        /// <summary>
        /// Gets the squared magnitude of the vector
        /// </summary>
        /// <param name="a">The vector</param>
        /// <returns>The squared magnitude</returns>
        public static float SqrMagnitude(PlaneVector a)
        {
            return (a.x * a.x) + (a.z * a.z);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is PlaneVector))
            {
                return false;
            }

            return PlaneVector.SqrMagnitude((PlaneVector)obj - this) < 9.99999944E-11f;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.z.GetHashCode() << 2;
        }
    }
}