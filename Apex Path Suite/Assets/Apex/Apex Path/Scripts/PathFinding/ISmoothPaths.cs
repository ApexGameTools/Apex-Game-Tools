/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.DataStructures;
    using Apex.WorldGeometry;

    /// <summary>
    /// Interface for path smoothers
    /// </summary>
    public interface ISmoothPaths
    {
        /// <summary>
        /// Smooths a path.
        /// </summary>
        /// <param name="goal">The goal node of the calculated path.</param>
        /// <param name="maxPathLength">Maximum length of the path.</param>
        /// <param name="request">The path request.</param>
        /// <param name="costStrategy">The cell cost provider.</param>
        /// <returns>The path in smoothed form</returns>
        Path Smooth(IPathNode goal, int maxPathLength, IPathRequest request, ICellCostStrategy costStrategy);
    }
}
