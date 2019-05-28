/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Interface for path nav options.
    /// </summary>
    public interface IPathNavigationOptions
    {
        /// <summary>
        /// The distance from the current destination node on the path at which the unit will switch to the next node.
        /// </summary>
        float nextNodeDistance { get; set; }

        /// <summary>
        /// The distance from the current way point at which the next way point will be requested
        /// </summary>
        float requestNextWaypointDistance { get; set; }

        /// <summary>
        /// Controls whether a <see cref="Apex.Messages.UnitNavigationEventMessage"/> is raised each time a node is reached.
        /// </summary>
        bool announceAllNodes { get; set; }

        /// <summary>
        /// The replan mode
        /// </summary>
        ReplanMode replanMode { get; set; }

        /// <summary>
        /// The replan interval
        /// When <see cref="replanMode"/> is <see cref="ReplanMode.AtInterval"/> the replan interval is the fixed interval in seconds between replanning.
        /// When <see cref="replanMode"/> is <see cref="ReplanMode.Dynamic"/> the replan interval is the minimum required time between each replan.
        /// </summary>
        float replanInterval { get; set; }
    }
}
