namespace Apex.DataStructures
{
    using System.Collections.Generic;

    /// <summary>
    /// Dedicated comparer for <see cref="PlaneVector"/>s
    /// </summary>
    public class PlaneVectorComparer : IEqualityComparer<PlaneVector>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The vector to compare.</param>
        /// <param name="y">The second vector to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(PlaneVector x, PlaneVector y)
        {
            return x == y;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(PlaneVector obj)
        {
            return obj.GetHashCode();
        }
    }
}