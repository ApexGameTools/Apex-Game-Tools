/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Path options component used to set both path finder and general path navigation options.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Path Options", 1028)]
    [ApexComponent("Unit Properties")]
    public class PathOptionsComponent : MonoBehaviour, IPathFinderOptions, IPathNavigationOptions
    {
        [SerializeField, Tooltip("The priority with which this unit's path requests should be processed.")]
        private int _pathingPriority = 0;

        [SerializeField, Tooltip("Whether to use path smoothing to create more natural paths.")]
        private bool _usePathSmoothing = true;

        [SerializeField, Tooltip("Whether to optimize by completely skipping path finding for paths where no obstruction exists between start and destination.\n\nPlease note that doing so will also skip portal evaluation inside the same grid, so e.g. short-cut portals within the same grid will not be evaluated in the case of unobstructed paths.")]
        private bool _optimizeUnobstructedPaths = true;

        [SerializeField, Tooltip("Whether to allow the path to cut corners. Corner cutting has slightly better performance, but produces less natural routes.")]
        private bool _allowCornerCutting = false;

        [SerializeField, Tooltip("Whether the unit is allowed to move to diagonal neighbours.")]
        private bool _preventDiagonalMoves = false;

        [SerializeField, Tooltip("Whether the unit will navigate to the nearest possible position if the actual destination is blocked or otherwise inaccessible. Using this with the A* engine is highly inefficient!")]
        private bool _navigateToNearestIfBlocked = false;

        [SerializeField, MinCheck(0, tooltip = "The maximum escape cell distance if the unit's starting position is blocked.")]
        private int _maxEscapeCellDistanceIfOriginBlocked = 3;

        [SerializeField, MinCheck(0.1f, tooltip = "The distance from the current destination node on the path at which the unit will switch to the next node.")]
        private float _nextNodeDistance = 1f;

        [SerializeField, MinCheck(0.2f, tooltip = "The distance from the current way point at which the next way point will be requested.")]
        private float _requestNextWaypointDistance = 2.0f;

        [SerializeField, Tooltip("Whether to raise a navigation event for each node reached along a path.")]
        private bool _announceAllNodes = false;

        [SerializeField, Tooltip("The replan mode.")]
        private ReplanMode _replanMode = ReplanMode.Dynamic;

        [SerializeField, MinCheck(0.1f, tooltip = "The time between replans, the exact meaning depends on the replan mode.")]
        private float _replanInterval = 0.5f;

        /// <summary>
        /// Gets or sets the priority with which this unit's path requests should be processed.
        /// </summary>
        /// <value>
        /// The pathing priority.
        /// </value>
        public int pathingPriority
        {
            get { return _pathingPriority; }
            set { _pathingPriority = value; }
        }

        /// <summary>
        /// Gets the maximum escape cell distance if origin blocked.
        /// This means that when starting a path and the origin (from position) is blocked, this determines how far away the pather will look for a free cell to escape to, before resuming the planned path.
        /// </summary>
        /// <value>
        /// The maximum escape cell distance if origin blocked.
        /// </value>
        public int maxEscapeCellDistanceIfOriginBlocked
        {
            get { return _maxEscapeCellDistanceIfOriginBlocked; }
            set { _maxEscapeCellDistanceIfOriginBlocked = value; }
        }

        /// <summary>
        /// Gets a value indicating whether to use path smoothing.
        /// Path smoothing creates more natural routes at a small cost to performance.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to path smoothing; otherwise, <c>false</c>.
        /// </value>
        public bool usePathSmoothing
        {
            get { return _usePathSmoothing; }
            set { _usePathSmoothing = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to optimize by completely skipping path finding for paths where no obstruction exists between start and destination.
        /// Please note that doing so will also skip portal evaluation inside the same grid, so e.g. short-cut portals within the same grid will not be evaluated in the case of unobstructed paths.
        /// </summary>
        /// <value>
        /// <c>true</c> to optimize unobstructed paths; otherwise, <c>false</c>.
        /// </value>
        public bool optimizeUnobstructedPaths
        {
            get { return _optimizeUnobstructedPaths; }
            set { _optimizeUnobstructedPaths = value; }
        }

        /// <summary>
        /// Gets a value indicating whether to allow the path to cut corners. Corner cutting has slightly better performance, but produces less natural routes.
        /// </summary>
        public bool allowCornerCutting
        {
            get { return _allowCornerCutting; }
            set { _allowCornerCutting = value; }
        }

        /// <summary>
        /// Gets a value indicating whether diagonal moves are prohibited.
        /// </summary>
        /// <value>
        /// <c>true</c> if diagonal moves are prohibited; otherwise, <c>false</c>.
        /// </value>
        public bool preventDiagonalMoves
        {
            get { return _preventDiagonalMoves; }
            set { _preventDiagonalMoves = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the unit will navigate to the nearest possible position if the actual destination is blocked or otherwise inaccessible.
        /// </summary>
        public bool navigateToNearestIfBlocked
        {
            get { return _navigateToNearestIfBlocked; }
            set { _navigateToNearestIfBlocked = value; }
        }

        /// <summary>
        /// The distance from the current destination node on the path at which the unit will switch to the next node.
        /// </summary>
        public float nextNodeDistance
        {
            get { return _nextNodeDistance; }
            set { _nextNodeDistance = value; }
        }

        /// <summary>
        /// The distance from the current way point at which the next way point will be requested
        /// </summary>
        public float requestNextWaypointDistance
        {
            get { return _requestNextWaypointDistance; }
            set { _requestNextWaypointDistance = value; }
        }

        /// <summary>
        /// Controls whether a <see cref="Apex.Messages.UnitNavigationEventMessage"/> is raised each time a node is reached.
        /// </summary>
        public bool announceAllNodes
        {
            get { return _announceAllNodes; }
            set { _announceAllNodes = value; }
        }

        /// <summary>
        /// The replan mode
        /// </summary>
        public ReplanMode replanMode
        {
            get { return _replanMode; }
            set { _replanMode = value; }
        }

        /// <summary>
        /// The replan interval
        /// When <see cref="replanMode"/> is <see cref="ReplanMode.AtInterval"/> the replan interval is the fixed interval in seconds between replanning.
        /// When <see cref="replanMode"/> is <see cref="ReplanMode.Dynamic"/> the replan interval is the minimum required time between each replan.
        /// </summary>
        public float replanInterval
        {
            get { return _replanInterval; }
            set { _replanInterval = value; }
        }

        /// <summary>
        /// Clones from one component to the other.
        /// </summary>
        /// <param name="optionsComponent">The component to clone from.</param>
        public void CloneFrom(PathOptionsComponent optionsComponent)
        {
            _pathingPriority = optionsComponent.pathingPriority;
            _usePathSmoothing = optionsComponent.usePathSmoothing;
            _optimizeUnobstructedPaths = optionsComponent.optimizeUnobstructedPaths;
            _allowCornerCutting = optionsComponent.allowCornerCutting;
            _preventDiagonalMoves = optionsComponent.preventDiagonalMoves;
            _navigateToNearestIfBlocked = optionsComponent.navigateToNearestIfBlocked;
            _maxEscapeCellDistanceIfOriginBlocked = optionsComponent.maxEscapeCellDistanceIfOriginBlocked;

            _nextNodeDistance = optionsComponent.nextNodeDistance;
            _requestNextWaypointDistance = optionsComponent.requestNextWaypointDistance;
            _announceAllNodes = optionsComponent.announceAllNodes;

            _replanMode = optionsComponent.replanMode;
            _replanInterval = optionsComponent.replanInterval;
        }
    }
}
