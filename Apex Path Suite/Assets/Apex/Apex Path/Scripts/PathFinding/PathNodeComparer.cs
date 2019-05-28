/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System.Collections.Generic;

    /// <summary>
    /// IComparer implementation for <see cref="IPathNode"/>s
    /// </summary>
    public sealed class PathNodeComparer : IComparer<IPathNode>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(IPathNode x, IPathNode y)
        {
            int compareCost = y.f.CompareTo(x.f);

            //tie breaker simply uses h, preferring a smaller h will prefer cells closer to the goal
            if (compareCost == 0)
            {
                return y.h.CompareTo(x.h);
            }

            return compareCost;
        }
    }
}
