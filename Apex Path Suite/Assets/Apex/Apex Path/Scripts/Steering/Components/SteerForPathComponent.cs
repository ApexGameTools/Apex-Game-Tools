/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.DataStructures;
    using Apex.Messages;
    using Apex.PathFinding;
    using Apex.Services;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to issue <see cref="IPathRequest"/>s and move along the resulting path.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Path", 1026)]
    [ApexComponent("Steering")]
    public class SteerForPathComponent : ArrivalBase, IMovable, INeedPath
    {
        /// <summary>
        /// The priority with which this unit's path requests should be processed.
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public int pathingPriority = 0;

        /// <summary>
        /// Whether to use path smoothing.
        /// Path smoothing creates more natural routes at a small cost to performance.
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public bool usePathSmoothing = true;

        /// <summary>
        /// Controls whether to allow the path to cut corners. Corner cutting has slightly better performance, but produces less natural routes.
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public bool allowCornerCutting = false;

        /// <summary>
        /// Controls whether navigation off-grid is prohibited.
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public bool preventOffGridNavigation = false;

        /// <summary>
        /// Controls whether the unit is allowed to move to diagonal neighbours.
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public bool preventDiagonalMoves = false;

        /// <summary>
        /// Controls whether the unit will navigate to the nearest possible position if the actual destination is blocked or otherwise inaccessible.
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public bool navigateToNearestIfBlocked = false;

        /// <summary>
        /// Gets the maximum escape cell distance if origin blocked.
        /// This means that when starting a path and the origin (from position) is blocked, this determines how far away the pather will look for a free cell to escape to, before resuming the planned path.
        /// </summary>
        [MinCheck(0)]
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public int maxEscapeCellDistanceIfOriginBlocked = 3;

        /// <summary>
        /// The distance from the current destination node on the path at which the unit will switch to the next node.
        /// </summary>
        [MinCheck(0.1f)]
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public float nextNodeDistance = 1f;

        /// <summary>
        /// The distance from the current way point at which the next way point will be requested
        /// </summary>
        [MinCheck(0f)]
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public float requestNextWaypointDistance = 2.0f;

        /// <summary>
        /// Controls whether a <see cref="Apex.Messages.UnitNavigationEventMessage"/> is raised each time a node is reached.
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public bool announceAllNodes = false;

        /// <summary>
        /// The replan mode
        /// </summary>
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public ReplanMode replanMode = ReplanMode.Dynamic;

        /// <summary>
        /// The replan interval
        /// When <see cref="replanMode"/> is <see cref="ReplanMode.AtInterval"/> the replan interval is the fixed interval in seconds between replanning.
        /// When <see cref="replanMode"/> is <see cref="ReplanMode.Dynamic"/> the replan interval is the minimum required time between each replan.
        /// </summary>
        [MinCheck(0.1f)]
        [Obsolete("Set on the PathFinderOptions component instead.")]
        public float replanInterval = 0.5f;

        /// <summary>
        /// Controls how the unit follows the path.
        /// If set to true, the unit will insist on getting within Next Node Distance of the next node on the path as specified on the Path Options Component,
        /// before moving on to the next node. If it overshoots it will move back.
        /// If set to false, the unit will proceed to the next node on the path once it passes current node. This will happen regardless of how far it is from it.
        /// Note that setting it to false can cause the unit to move into obstacles if it is off its path for whatever reason.
        /// </summary>
        [Tooltip("Controls how the unit follows the path.\nIf set to true, the unit will insist on getting within Next Node Distance of the next node on the path as specified on the Path Options Component; otherwise it will proceed once it passes by its current next node.")]
        public bool strictPathFollowing = true;

        private readonly object _syncLock = new object();
        private IUnitFacade _unit;
        private IPathNavigationOptions _pathSettings;
        private IPathRequest _pendingPathRequest;
        private PathResult _pendingResult;
        private Path _currentPath;
        private Vector3 _endOfResolvedPath;
        private ReplanCallback _manualReplan;
        private IPositioned _currentDestination;
        private float _remainingPathDistance;
        private float _remainingTotalDistance;
        private IGrid _currentGrid;
        private Transform _transform;

        private Vector3 _curPlannedDirection;

        private bool _stop;
        private bool _blockMoveOrders;
        private bool _stopped;
        private bool _isPortaling;
        private float _lastPathRequestTime;

        private UnitNavigationEventMessage _navMessage;
        private UnitNavigationEventMessage.Event _arrivalEvent;
        private UnitNavigationEventMessage.Event _lastNavigationEvent;
        private IProcessPathResults[] _resultProcessors;

        private WaypointList _wayPoints;

        /// <summary>
        /// Gets the latest arrival event.
        /// </summary>
        /// <value>
        /// The latest arrival event.
        /// </value>
        public UnitNavigationEventMessage.Event lastNavigationEvent
        {
            get { return _lastNavigationEvent; }
        }

        /// <summary>
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The current path.
        /// </value>
        public Path currentPath
        {
            get { return _currentPath; }
        }

        /// <summary>
        /// Gets the current way points.
        /// </summary>
        /// <value>
        /// The current way points.
        /// </value>
        public IIterable<Vector3> currentWaypoints
        {
            get { return _wayPoints; }
        }

        /// <summary>
        /// Gets the final destination, which is either the last point in the <see cref="currentPath" /> or the last of the <see cref="currentWaypoints" /> if there are any.
        /// </summary>
        /// <value>
        /// The final destination.
        /// </value>
        public Vector3? finalDestination
        {
            get
            {
                if (_wayPoints.count > 0)
                {
                    return _wayPoints.last;
                }

                if (_currentPath != null && _currentPath.count > 0)
                {
                    return _currentPath.Last().position;
                }

                if (_currentDestination != null)
                {
                    return _currentDestination.position;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the position of the next node along the path currently being moved towards.
        /// </summary>
        /// <value>
        /// The next node position.
        /// </value>
        public Vector3? nextNodePosition
        {
            get
            {
                if (_currentDestination != null)
                {
                    return _currentDestination.position;
                }

                return null;
            }
        }

        /// <summary>
        /// Called on Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            this.WarnIfMultipleInstances();

            _transform = this.transform;

            _wayPoints = new WaypointList();
            _navMessage = new UnitNavigationEventMessage(this.gameObject);

            _resultProcessors = this.GetComponents<SteerForPathResultProcessorComponent>();
            Array.Sort(_resultProcessors, (a, b) => a.processingOrder.CompareTo(b.processingOrder));

            _unit = this.GetUnitFacade();
            _pathSettings = _unit.pathNavigationOptions;

            _stopped = true;
        }

        /// <summary>
        /// Asks the object to move to the specified position
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        public void MoveTo(Vector3 position, bool append)
        {
            if (_blockMoveOrders)
            {
                return;
            }

            //If this is a way point and we are already moving, just queue it up
            if (append && !_stopped)
            {
                _wayPoints.AddWaypoint(position);
                return;
            }

            var from = _isPortaling ? _currentDestination.position : _transform.position;

            //Either we don't have a request or this is the first point in a way point route
            StopInternal();

            _wayPoints.AddWaypoint(position, true);
            _lastNavigationEvent = UnitNavigationEventMessage.Event.None;
            RequestPath(from, position, InternalPathRequest.Type.Normal);
        }

        /// <summary>
        /// Asks the object to move to the specified position, via a set of waypoints
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="via">A list of waypoints to visit along the route to position.</param>
        public void MoveTo(Vector3 position, IEnumerable<Vector3> via)
        {
            if (_blockMoveOrders)
            {
                return;
            }

            var from = _isPortaling ? _currentDestination.position : _transform.position;

            //Either we don't have a request or this is the first point in a way point route
            StopInternal();

            var firstWaypoint = position;
            if (via != null && via.Any())
            {
                firstWaypoint = via.First();
                _wayPoints.AddWaypoint(firstWaypoint, true);

                foreach (var wp in via.Skip(1))
                {
                    _wayPoints.AddWaypoint(wp);
                }

                _wayPoints.AddWaypoint(position);
            }
            else
            {
                _wayPoints.AddWaypoint(firstWaypoint, true);
            }

            _lastNavigationEvent = UnitNavigationEventMessage.Event.None;
            RequestPath(from, firstWaypoint, InternalPathRequest.Type.Normal);
        }

        /// <summary>
        /// Asks the object to move along the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void MoveAlong(Path path)
        {
            MoveAlong(path, null);
        }

        /// <summary>
        /// Asks the object to move along the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        public void MoveAlong(Path path, ReplanCallback onReplan)
        {
            Ensure.ArgumentNotNull(path, "path");

            StopInternal();

            _lastNavigationEvent = UnitNavigationEventMessage.Event.None;
            SetManualPath(path, onReplan);
        }

        /// <summary>
        /// Enables the movement orders following a call to <see cref="DisableMovementOrders" />.
        /// </summary>
        public void EnableMovementOrders()
        {
            _blockMoveOrders = false;
        }

        /// <summary>
        /// Disables movement orders, e.g. calls to <see cref="MoveTo(Vector3, bool)" /> will be ignored until <see cref="EnableMovementOrders" /> is called.
        /// </summary>
        public void DisableMovementOrders()
        {
            _blockMoveOrders = true;
        }

        /// <summary>
        /// Stop following the path.
        /// </summary>
        public override void Stop()
        {
            _lastNavigationEvent = UnitNavigationEventMessage.Event.StoppedByRequest;
            _stop = true;
        }

        /// <summary>
        /// Replans the path.
        /// </summary>
        public void ReplanPath()
        {
            if (_stopped || _pendingPathRequest != null)
            {
                return;
            }

            var pathToReplan = _currentPath;

            if (_manualReplan != null)
            {
                var updatedPath = _manualReplan(this.gameObject, _currentDestination, pathToReplan);
                _wayPoints.Clear();
                SetManualPath(updatedPath, _manualReplan);
            }
            else
            {
                RequestPath(_transform.position, _wayPoints.desiredEndOfPath, InternalPathRequest.Type.Normal);
            }
        }

        //Note this may be called from another thread if the PathService is running asynchronously
        void INeedPath.ConsumePathResult(PathResult result)
        {
            lock (_syncLock)
            {
                //If we have stooped or get back the result of a request other than the one we currently expect, just toss it as it will be outdated.
                if (_stopped || result.originalRequest != _pendingPathRequest)
                {
                    return;
                }

                _pendingResult = result;
                _pendingPathRequest = null;
            }
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (_isPortaling)
            {
                output.pause = true;
                return;
            }

            if (_stopped)
            {
                return;
            }

            if (_stop)
            {
                StopInternal();
                return;
            }

            if (!ResolveNextPoint())
            {
                return;
            }

            //Handle waypoints
            if (_wayPoints.hasPendingWaypoint && _pendingPathRequest == null)
            {
                //waypoint request are done if we are close to the point where we need to slow down for arrival as we want to consider the entire length of the path including waypoints when deciding when to slow down.
                if (_remainingTotalDistance < this.slowingDistance + _pathSettings.requestNextWaypointDistance)
                {
                    //For way points we cannot use the desired end of path, but must use the actual end as the starting point.
                    RequestPath(_endOfResolvedPath, _wayPoints.NextWaypoint(), InternalPathRequest.Type.Waypoint);
                }
            }

            HandlePathReplan();

            if (Arrive(_currentDestination.position, _remainingTotalDistance, input, output))
            {
                if (_pendingPathRequest != null)
                {
                    return;
                }

                //TODO: this should be set whenever the unit stops, however since it may not be the only locomotion component a smarter way must be found
                //The SterrableUnitComponent ought to handle this, so if there is no locomotion component that have out put it is set to true; otherwise false.
                //Also change the description of the property
                _unit.hasArrivedAtDestination = true;
                StopAndAnnounceArrival();
            }
        }

        /// <summary>
        /// Requests the path.
        /// </summary>
        /// <param name="request">The request.</param>
        public void RequestPath(IPathRequest request)
        {
            var unit = this.GetUnitFacade();

            request = InternalPathRequest.Internalize(request);
            request.timeStamp = Time.time;

            lock (_syncLock)
            {
                request.requester = this;
                request.requesterProperties = unit;

                if (_pendingPathRequest != null)
                {
                    _pendingPathRequest.hasDecayed = true;
                }

                _pendingPathRequest = request;

                _stop = false;
                _stopped = false;
            }

            GameServices.pathService.QueueRequest(_pendingPathRequest, unit.pathFinderOptions.pathingPriority);
        }

        private void RequestPath(Vector3 from, Vector3 to, InternalPathRequest.Type type)
        {
            var unit = this.GetUnitFacade();

            lock (_syncLock)
            {
                if (_pendingPathRequest != null)
                {
                    _pendingPathRequest.hasDecayed = true;
                }

                _pendingPathRequest = new InternalPathRequest
                {
                    from = from,
                    to = to,
                    pathType = type,
                    requester = this,
                    requesterProperties = unit,
                    pathFinderOptions = unit.pathFinderOptions,
                    timeStamp = Time.time
                };

                if (type == InternalPathRequest.Type.Normal)
                {
                    _pendingPathRequest.via = _wayPoints.GetViaPoints();
                }

                _stop = false;
                _stopped = false;
            }

            GameServices.pathService.QueueRequest(_pendingPathRequest, unit.pathFinderOptions.pathingPriority);
        }

        private void SetManualPath(Path path, ReplanCallback onReplan)
        {
            if (path == null || path.count == 0)
            {
                StopInternal();
                return;
            }

            _stop = false;
            _stopped = false;
            _arrivalEvent = UnitNavigationEventMessage.Event.DestinationReached;
            _manualReplan = onReplan;

            int pathCount = path.count - 1;
            for (int i = 0; i < pathCount; i++)
            {
                var node = path[i];
                if (node is Waypoint)
                {
                    _wayPoints.AddWaypoint(node.position);
                }
            }

            _wayPoints.AddWaypoint(path.Last().position, true);

            SetPath(path, false);

            _lastPathRequestTime = Time.time;
        }

        private void AnnounceEvent(UnitNavigationEventMessage.Event e, Vector3 destination, Vector3[] pendingWaypoints)
        {
            _lastNavigationEvent = e;

            _navMessage.isHandled = false;
            _navMessage.eventCode = e;
            _navMessage.destination = destination;
            _navMessage.pendingWaypoints = pendingWaypoints ?? Empty<Vector3>.array;
            GameServices.messageBus.Post(_navMessage);
        }

        private bool ResolveNextPoint()
        {
            if (_pendingResult != null)
            {
                ConsumeResult();
            }

            if (_currentPath == null)
            {
                return false;
            }

            //Get the direction of movement and the remaining distance
            var actualDirection = _transform.position.DirToXZ(_currentDestination.position);
            var currentDestinationDistance = actualDirection.magnitude;

            _remainingTotalDistance = _remainingPathDistance + currentDestinationDistance;

            //If we are on the last node already, there is no more to do here.
            if (_currentPath.count == 0)
            {
                return true;
            }

            //Are we at a place where we need to get the next node?
            if (currentDestinationDistance > _pathSettings.nextNodeDistance &&
                (this.strictPathFollowing || Vector3.Dot(_curPlannedDirection, actualDirection) > 0f))
            {
                return true;
            }

            var previousDestination = _currentDestination;
            _currentDestination = _currentPath.Pop();

            var portal = _currentDestination as IPortalNode;
            if (portal != null)
            {
                _isPortaling = true;

                //Since a portal will never be the last node on a path, we can safely pop the next in line as the actual destination of the portal
                //doing it like this also caters for the scenario where the destination is the last node.
                _currentDestination = _currentPath.Pop();

                _currentGrid = portal.Execute(
                    _transform,
                    _currentDestination,
                    () =>
                    {
                        _isPortaling = false;
                    });
            }

            if (previousDestination != null)
            {
                _remainingPathDistance -= (_currentDestination.position - previousDestination.position).magnitude;

                if (previousDestination is Waypoint)
                {
                    _wayPoints.ConsumeViaPoint();
                    AnnounceEvent(UnitNavigationEventMessage.Event.WaypointReached, previousDestination.position, null);
                }
                else if (_pathSettings.announceAllNodes)
                {
                    AnnounceEvent(UnitNavigationEventMessage.Event.NodeReached, previousDestination.position, null);
                }
            }

            _curPlannedDirection = _transform.position.DirToXZ(_currentDestination.position);

            return !_isPortaling;
        }

        private void ConsumeResult()
        {
            //Since result processing may actually repath and consequently a new result may arrive we need to operate on locals and null the pending result
            PathResult result;
            lock (_syncLock)
            {
                result = _pendingResult;
                _pendingResult = null;
            }

            var req = result.originalRequest as InternalPathRequest;

            //Consume way points if appropriate. This must be done prior to the processing of the result, since if the request was a way point request, the first item in line is the one the result concerns.
            bool isWaypoint = req.pathType == InternalPathRequest.Type.Waypoint;
            if (!isWaypoint)
            {
                //Since a waypoint request does not account for the entire path, we do not set the request time
                _lastPathRequestTime = req.timeStamp;
            }

            //Process the result
            if (ProcessAndValidateResult(result, isWaypoint))
            {
                //Consume the result
                SetPath(result.path, isWaypoint);
            }
        }

        private bool TrimPath(Path path)
        {
            //Make sure that the path was resolved before moving too far from where it was requested.
            var nn = _pathSettings.nextNodeDistance;
            if (_unit.position.DirToXZ(path.Peek().position).sqrMagnitude < nn * nn)
            {
                return true;
            }

            //First thing to do is to remove any waypoints that have already been reached.
            //Since the actual waypoints may differ from the requested ones (if they were corrected to allow the unit to access them)
            //we cannot remove them by comparison.
            //Since the last point is now always a waypoint (but not a via point), we skip that as obviously the last point cannot be pruned.
            int currentWaypointCount = _wayPoints.viaPointsCount;
            int pathCount = path.count;
            int encounteredWaypoints = 0;
            for (int i = pathCount - 2; i >= 0; i--)
            {
                var node = path[i];
                if (node is Waypoint)
                {
                    if (encounteredWaypoints == currentWaypointCount)
                    {
                        path.Truncate(i + 1);
                        break;
                    }

                    encounteredWaypoints++;
                }
            }

            //Next find the last node on the path to which we can move directly, i.e. skipping nodes already reached.
            var gridBounds = _currentGrid.bounds;
            var matrix = _currentGrid.cellMatrix;
            var costStrategy = GameServices.cellCostStrategy;

            int firstNodeIdx = -1;
            pathCount = path.count;
            for (int i = 0; i < pathCount; i++)
            {
                //If we haven't yet found an accessible node move on if the inspected node is outside the current grid or is a portal.
                //Otherwise stop, we do not move past portals.
                var node = path[i];
                if (node is IPortalNode || !gridBounds.Contains(node.position))
                {
                    if (firstNodeIdx < 0)
                    {
                        continue;
                    }

                    break;
                }

                if (PathSmoother.CanReducePath(node, _unit, _unit, matrix, costStrategy))
                {
                    firstNodeIdx = i;
                }
                else if (firstNodeIdx >= 0)
                {
                    break;
                }

                //We must visit waypoint so they cannot be smoothed away
                if (node is Waypoint)
                {
                    break;
                }
            }

            if (firstNodeIdx < 0)
            {
                return false;
            }

            path.Truncate(firstNodeIdx);
            return true;
        }

        private void SetPath(Path path, bool isWaypoint)
        {
            _endOfResolvedPath = path.Last().position;
            _currentGrid = GridManager.instance.GetGrid(path.Peek().position);

            if (isWaypoint)
            {
                _remainingPathDistance += path.CalculateLength();

                _currentPath = _currentPath.AppendSegment(path);
            }
            else
            {
                if (!TrimPath(path))
                {
                    RequestPath(_transform.position, _wayPoints.desiredEndOfPath, InternalPathRequest.Type.Normal);
                    _currentPath = null;
                    return;
                }

                _currentPath = path;
                _remainingPathDistance = path.CalculateLength();

                //Pop the first node as our next destination.
                _currentDestination = path.Pop();
                _curPlannedDirection = _transform.position.DirToXZ(_currentDestination.position);
            }

            _unit.hasArrivedAtDestination = false;
        }

        private bool ProcessAndValidateResult(PathResult result, bool isWaypoint)
        {
            _wayPoints.frozen = false;

            for (int i = 0; i < _resultProcessors.Length; i++)
            {
                if (_resultProcessors[i].HandleResult(result, this))
                {
                    return (result.status == PathingStatus.Complete);
                }
            }

            var status = result.status;
            bool isPartial = (status == PathingStatus.CompletePartial);
            if (isPartial)
            {
                status = result.innerResult.status;
            }

            switch (status)
            {
                case PathingStatus.Complete:
                {
                    /* All is good, no more to do */
                    _arrivalEvent = UnitNavigationEventMessage.Event.DestinationReached;
                    return true;
                }

                case PathingStatus.NoRouteExists:
                case PathingStatus.EndOutsideGrid:
                {
                    _arrivalEvent = UnitNavigationEventMessage.Event.StoppedNoRouteExists;
                    break;
                }

                case PathingStatus.StartOutsideGrid:
                {
                    _arrivalEvent = UnitNavigationEventMessage.Event.StoppedUnitOutsideGrid;
                    break;
                }

                case PathingStatus.DestinationBlocked:
                {
                    _arrivalEvent = UnitNavigationEventMessage.Event.StoppedDestinationBlocked;
                    break;
                }

                case PathingStatus.Decayed:
                {
                    //We cannot reissue the request here, since we may be on a different thread, but then again why would we issue the request again if it had a decay threshold its no longer valid.
                    _arrivalEvent = UnitNavigationEventMessage.Event.StoppedRequestDecayed;
                    break;
                }

                case PathingStatus.Failed:
                {
                    StopInternal();
                    Debug.LogError("Path request failed: " + result.errorInfo);
                    break;
                }
            }

            _wayPoints.frozen = true;

            if (isPartial)
            {
                return true;
            }

            if (!isWaypoint)
            {
                //If we are not already moving stop and announce here
                StopAndAnnounceArrival();
            }

            return false;
        }

        private void StopAndAnnounceArrival()
        {
            Vector3 arrivalPosition = Vector3.zero;
            Vector3[] pendingWaypoints = null;
            if (_arrivalEvent == UnitNavigationEventMessage.Event.DestinationReached)
            {
                arrivalPosition = _currentDestination.position;
            }
            else if (_wayPoints.count > 0)
            {
                pendingWaypoints = _wayPoints.GetPending();
                arrivalPosition = pendingWaypoints[0];
            }

            StopInternal();
            AnnounceEvent(_arrivalEvent, arrivalPosition, pendingWaypoints);
        }

        private void HandlePathReplan()
        {
            //If we are moving entirely off grid, there is no point in replanning, as there is nothing to replan on.
            if (_currentGrid == null || _pathSettings.replanMode == ReplanMode.NoReplan)
            {
                return;
            }

            var now = Time.time;
            if (now - _lastPathRequestTime < _pathSettings.replanInterval)
            {
                return;
            }

            bool replan = true;
            if (_pathSettings.replanMode == ReplanMode.Dynamic)
            {
                replan = _currentGrid.HasSectionsChangedSince(_transform.position, _lastPathRequestTime);
            }

            if (replan)
            {
                ReplanPath();
            }
        }

        private void StopInternal()
        {
            lock (_syncLock)
            {
                if (_pendingPathRequest != null)
                {
                    _pendingPathRequest.hasDecayed = true;
                }

                _stopped = true;
                _wayPoints.Clear();
                _currentPath = null;
                _pendingPathRequest = null;
                _currentDestination = null;
                _pendingResult = null;
                _manualReplan = null;
            }
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="steerForPath">The component to clone from.</param>
        public void CloneFrom(SteerForPathComponent steerForPath)
        {
            this.priority = steerForPath.priority;
            this.weight = steerForPath.weight;

            this.slowingAlgorithm = steerForPath.slowingAlgorithm;
            this.autoCalculateSlowingDistance = steerForPath.autoCalculateSlowingDistance;
            this.slowingDistance = steerForPath.slowingDistance;
            this.arrivalDistance = steerForPath.arrivalDistance;

            this.strictPathFollowing = steerForPath.strictPathFollowing;
        }

        private class InternalPathRequest : BasicPathRequest
        {
            internal enum Type
            {
                Normal,
                Waypoint
            }

            internal Type pathType
            {
                get;
                set;
            }

            internal static InternalPathRequest Internalize(IPathRequest request)
            {
                var internalized = request as InternalPathRequest;
                if (internalized == null)
                {
                    internalized = new InternalPathRequest();
                    Utils.CopyProps(request, internalized);
                }

                internalized.pathType = Type.Normal;
                return internalized;
            }
        }
    }
}