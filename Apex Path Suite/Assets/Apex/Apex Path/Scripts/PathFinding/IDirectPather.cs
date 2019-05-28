/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Interface for the direct (pre pathfinding) pather
    /// </summary>
    public interface IDirectPather
    {
        /// <summary>
        /// Resolves the direct path or delegates the request on to path finding.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <returns>A path request to use in path finding or null if the path was resolved.</returns>
        IPathRequest ResolveDirectPath(IPathRequest req);
    }
}
