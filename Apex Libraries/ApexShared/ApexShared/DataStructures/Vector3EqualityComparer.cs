/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Equality comparer for Vector3s with equality threshold.
    /// </summary>
    public class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        private float _equalityThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3EqualityComparer"/> class.
        /// </summary>
        /// <param name="equalityThreshold">The equality threshold, i.e. if the square magnitude of the difference between two vectors falls below this, they are considered equal.</param>
        public Vector3EqualityComparer(float equalityThreshold)
        {
            _equalityThreshold = equalityThreshold;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first Vector to compare.</param>
        /// <param name="y">The second Vector to compare.</param>
        /// <returns>
        /// true if the specified Vectors are equal; otherwise, false.
        /// </returns>
        public bool Equals(Vector3 x, Vector3 y)
        {
            return (x - y).sqrMagnitude < _equalityThreshold;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The Vector.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }
}
