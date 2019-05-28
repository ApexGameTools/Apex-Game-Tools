/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Interface for path finder options
    /// </summary>
    public interface IPathFinderOptions
    {
        /// <summary>
        /// Gets or sets the priority with which this unit's path requests should be processed.
        /// </summary>
        /// <value>
        /// The pathing priority.
        /// </value>
        int pathingPriority { get; set; }

        /// <summary>
        /// Gets or sets the maximum escape cell distance if origin blocked.
        /// This means that when starting a path and the origin (from position) is blocked, this determines how far away the pather will look for a free cell to escape to, before resuming the planned path.
        /// </summary>
        /// <value>
        /// The maximum escape cell distance if origin blocked.
        /// </value>
        int maxEscapeCellDistanceIfOriginBlocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use path smoothing.
        /// Path smoothing creates more natural routes at a small cost to performance.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to path smoothing; otherwise, <c>false</c>.
        /// </value>
        bool usePathSmoothing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to optimize by completely skipping path finding for paths where no obstruction exists between start and destination.
        /// Please note that doing so will also skip portal evaluation inside the same grid, so e.g. short-cut portals within the same grid will not be evaluated in the case of unobstructed paths.
        /// </summary>
        /// <value>
        /// <c>true</c> to optimize unobstructed paths; otherwise, <c>false</c>.
        /// </value>
        bool optimizeUnobstructedPaths { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow the path to cut corners. Corner cutting has slightly better performance, but produces less natural routes.
        /// </summary>
        bool allowCornerCutting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether diagonal moves are prohibited.
        /// </summary>
        /// <value>
        /// <c>true</c> if diagonal moves are prohibited; otherwise, <c>false</c>.
        /// </value>
        bool preventDiagonalMoves { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the unit will navigate to the nearest possible position if the actual destination is blocked or otherwise inaccessible.
        /// </summary>
        bool navigateToNearestIfBlocked { get; set; }
    }
}
