/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System.Collections.Generic;

    /// <summary>
    /// Comparer for scored options.
    /// </summary>
    public struct ScoredOptionComparer<T> : IComparer<ScoredOption<T>>
    {
        /// <summary>
        /// Whether or not to reverse comparison, i.e. descending sort.
        /// </summary>
        public bool descending;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoredOptionComparer{T}"/> struct.
        /// </summary>
        /// <param name="descending">if set to <c>true</c> comparison is descending.</param>
        public ScoredOptionComparer(bool descending = false)
        {
            this.descending = descending;
        }

        /// <summary>
        /// Compares the specified two options.
        /// </summary>
        /// <param name="x">The first option.</param>
        /// <param name="y">The second option.</param>
        /// <returns>The comparison value, -1, 0 or 1</returns>
        public int Compare(ScoredOption<T> x, ScoredOption<T> y)
        {
            var compare = x.score.CompareTo(y.score);
            return this.descending ? -1 * compare : compare;
        }
    }
}
