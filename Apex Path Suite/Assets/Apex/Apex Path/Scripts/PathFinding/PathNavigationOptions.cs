/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Default path nav options implementation.
    /// </summary>
    public class PathNavigationOptions : IPathNavigationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathNavigationOptions"/> class.
        /// </summary>
        public PathNavigationOptions()
        {
            this.nextNodeDistance = 1f;
            this.requestNextWaypointDistance = 2f;
            this.replanMode = ReplanMode.Dynamic;
            this.replanInterval = 0.5f;
        }

        /// <summary>
        /// The distance from the current destination node on the path at which the unit will switch to the next node.
        /// </summary>
        public float nextNodeDistance
        {
            get;
            set;
        }

        /// <summary>
        /// The distance from the current way point at which the next way point will be requested
        /// </summary>
        public float requestNextWaypointDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Controls whether a <see cref="Apex.Messages.UnitNavigationEventMessage" /> is raised each time a node is reached.
        /// </summary>
        public bool announceAllNodes
        {
            get;
            set;
        }

        /// <summary>
        /// The replan mode
        /// </summary>
        public ReplanMode replanMode
        {
            get;
            set;
        }

        /// <summary>
        /// The replan interval
        /// When <see cref="replanMode" /> is <see cref="ReplanMode.AtInterval" /> the replan interval is the fixed interval in seconds between replanning.
        /// When <see cref="replanMode" /> is <see cref="ReplanMode.Dynamic" /> the replan interval is the minimum required time between each replan.
        /// </summary>
        public float replanInterval
        {
            get;
            set;
        }
    }
}
