/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.Debugging;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.PathFinding;
    using Apex.Services;
    using Apex.Steering;
    using Apex.Steering.Components;
    using Apex.Steering.HeightNavigation;
    using Apex.Steering.VectorFields;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// The default group type used for Apex Steer.
    /// Requires vector fields.
    /// </summary>
    public class DefaultSteeringTransientUnitGroup : DefaultTransientUnitGroup, ILoadBalanced, INeedPath
    {
        private ReplanMode _replanMode;
        private float _replanInterval;
        private float _nextNodeDistance;
        private float _requestNextWaypointDistance;

        private IPathRequest _pendingPathRequest;
        private PathResult _pendingResult;
        private Path _currentPath;
        private SimpleQueue<Vector3> _wayPoints;
        private UnitNavigationEventMessage _navMessage;
        private ReplanCallback _manualReplan;
        private float _lastPathRequestTime;
        private IGrid _currentGrid;
        private IVectorField _vectorField;

        private Vector3 _groupCog;

        private bool _blockMoveOrders;
        private bool _wait;
        private bool _stopped;

        private bool _announcedEndOnce;

        private IUnitFacade _modelUnit;

        private List<FormationPositionWithIndex> _tempFormationPositions;
        private DynamicArray<Vector3> _formationPositions;
        private DistanceComparer<IUnitFacade> _unitSortComparer;
        private IFormation _currentFormation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSteeringTransientUnitGroup"/> class.
        /// </summary>
        /// <param name="capacity">The capacity to use for pre-allocation of the members list.</param>
        public DefaultSteeringTransientUnitGroup(int capacity)
            : base(capacity)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSteeringTransientUnitGroup"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public DefaultSteeringTransientUnitGroup(IUnitFacade[] members)
            : base(members)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSteeringTransientUnitGroup"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public DefaultSteeringTransientUnitGroup(IEnumerable<IUnitFacade> members)
            : base(members)
        {
            Initialize();
        }

        /// <summary>
        /// Gets the group's center of gravity - i.e. all members' position averaged.
        /// </summary>
        public override Vector3 centerOfGravity
        {
            get { return _groupCog; }
        }

        /// <summary>
        /// Gets the vector field currently associated with the steering group.
        /// </summary>
        public IVectorField vectorField
        {
            get { return _vectorField; }
        }

        /// <summary>
        /// Gets the current active path associated with the steering group.
        /// </summary>
        public override Path currentPath
        {
            get { return _currentPath; }
        }

        /// <summary>
        /// For internal use only. Gets or sets the next group - used in portalling, where units leave their old group and join a new one.
        /// </summary>
        internal DefaultSteeringTransientUnitGroup nextGroup
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current waypoints associated with the steering group.
        /// </summary>
        public override IIterable<Vector3> currentWaypoints
        {
            get { return _wayPoints; }
        }

        /// <summary>
        /// Gets the final destination - i.e. the last path node or last waypoint.
        /// </summary>
        public override Vector3? finalDestination
        {
            get
            {
                // otherwise return the last waypoint
                if (_wayPoints != null && _wayPoints.count > 0)
                {
                    return _wayPoints.Last();
                }

                // return last node in path, if path exists and has nodes
                if (_currentPath != null && _currentPath.count > 0)
                {
                    return _currentPath.Last().position;
                }

                if (_vectorField != null && _vectorField.nextNodePosition != null)
                {
                    return _vectorField.nextNodePosition.position;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the next node position - i.e. the next node position, according to the currently associated vector field (if not null)
        /// <returns>Null if the vector field is null, if the vector field path is null or if the vector field path count is 0</returns>.
        /// </summary>
        public override Vector3? nextNodePosition
        {
            get
            {
                if (_vectorField == null || _vectorField.nextNodePosition == null)
                {
                    // if the vector field or its path is null or empty, then return null
                    return null;
                }

                return _vectorField.nextNodePosition.position;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to repeatedly update this group each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        public virtual bool repeat
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the model unit.
        /// </summary>
        /// <value>
        /// The model unit.
        /// </value>
        public override IUnitFacade modelUnit
        {
            get
            {
                if (this.count == 0)
                {
                    return null;
                }

                return _modelUnit;
            }
        }

        /// <summary>
        /// Gets the current formation.
        /// </summary>
        /// <value>
        /// The current formation.
        /// </value>
        public override IFormation currentFormation
        {
            get { return _currentFormation; }
        }

        private void Initialize()
        {
            _wayPoints = new SimpleQueue<Vector3>();
            _groupCog = GetGroupCenterOfGravity();

            _unitSortComparer = new DistanceComparer<IUnitFacade>(false);

            _stopped = true;
            var updateInterval = GameServices.navigationSettings.groupUpdateInterval;

            NavLoadBalancer.steering.Add(this, updateInterval, true);
        }

        private void InitializeModelUnitSettings()
        {
            var pathNavOptions = _modelUnit != null ? _modelUnit.pathNavigationOptions : this[0].pathNavigationOptions;

            // cache locally the relevant path navigation options
            _replanMode = pathNavOptions.replanMode;
            _replanInterval = pathNavOptions.replanInterval;
            _nextNodeDistance = pathNavOptions.nextNodeDistance;
            _requestNextWaypointDistance = pathNavOptions.requestNextWaypointDistance;

            _navMessage = new UnitNavigationEventMessage(_modelUnit != null ? _modelUnit.gameObject : this[0].gameObject);
        }

        /// <summary>
        /// Asks the object to move to the specified position
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        protected override void MoveToInternal(Vector3 position, bool append)
        {
            if (_blockMoveOrders)
            {
                // if move orders are blocked, then stop here
                return;
            }

            if (append && !_stopped)
            {
                // append waypoints if we are moving and supposed to be appending
                _wayPoints.Enqueue(position);
                return;
            }

            StopInternal();
            RequestPath(position, InternalPathRequest.Type.Normal);
        }

        /// <summary>
        /// Asks the object to move along the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        protected override void MoveAlongInternal(Path path, ReplanCallback onReplan)
        {
            if (_blockMoveOrders)
            {
                // if move orders are blocked, then stop here
                return;
            }

            Ensure.ArgumentNotNull(path, "path");
            StopInternal();
            SetManualPath(path, onReplan);
        }

        /// <summary>
        /// Waits the specified seconds before continuing the move.
        /// </summary>
        /// <param name="seconds">The seconds to wait or null to wait until explicitly <see cref="Resume" />d.</param>
        public override void Wait(float? seconds)
        {
            _wait = true;

            if (seconds.HasValue)
            {
                NavLoadBalancer.defaultBalancer.Add(new OneTimeAction((ignored) => this.Resume()), seconds.Value, true);
            }

            if (this.modelUnit != null)
            {
                this.modelUnit.Wait(null);
            }

            // delegate the Wait call through to all members
            int membersCount = this.count;
            for (int i = 0; i < membersCount; i++)
            {
                this[i].Wait(null);
            }
        }

        /// <summary>
        /// Resumes movements after a <see cref="Wait" />.
        /// </summary>
        public override void Resume()
        {
            _wait = false;

            if (this.modelUnit != null)
            {
                this.modelUnit.Resume();
            }

            // delegate the Resume call through to all members
            int membersCount = this.count;
            for (int i = 0; i < membersCount; i++)
            {
                this[i].Resume();
            }
        }

        /// <summary>
        /// Enables the movement orders following a call to <see cref="DisableMovementOrders" />.
        /// </summary>
        public override void EnableMovementOrders()
        {
            _blockMoveOrders = false;
        }

        /// <summary>
        /// Disables movement orders, i.e. calls to <see cref="IMovable.MoveTo" /> and <see cref="IMovable.MoveAlong(Path)"/> will be ignored until <see cref="EnableMovementOrders" /> is called.
        /// </summary>
        public override void DisableMovementOrders()
        {
            _blockMoveOrders = true;
        }

        /// <summary>
        /// Stop following the path.
        /// </summary>
        public override void Stop()
        {
            StopInternal();

            if (this.modelUnit != null)
            {
                this.modelUnit.Stop();
            }

            // delegate the Stop call through to all members
            int membersCount = this.count;
            for (int i = 0; i < membersCount; i++)
            {
                this[i].Stop();
            }
        }

        /// <summary>
        /// Requests a path.
        /// </summary>
        /// <param name="request">The request.</param>
        public void RequestPath(IPathRequest request)
        {
            if (this.count == 0)
            {
                // if the group has no members, then no need to request anything
                return;
            }

            request = InternalPathRequest.Internalize(request);
            request.requester = this;
            request.requesterProperties = _modelUnit;

            _pendingPathRequest = request;

            _stopped = false;
            _lastPathRequestTime = Time.time;
            GameServices.pathService.QueueRequest(_pendingPathRequest, _modelUnit.pathFinderOptions.pathingPriority);
        }

        private void RequestPath(Vector3 to, InternalPathRequest.Type type)
        {
            if (this.count == 0)
            {
                // if the group has no members, then no need to request anything
                return;
            }

            // find a valid from position for the group
            Vector3 fromPos = _modelUnit.position;
            var grid = GridManager.instance.GetGrid(fromPos);
            Cell fromCell = grid.GetCell(fromPos, true);
            if (fromCell == null || !fromCell.IsWalkableWithClearance(_modelUnit))
            {
                fromCell = grid.GetNearestWalkableCell(fromPos, fromPos, false, _modelUnit.pathFinderOptions.maxEscapeCellDistanceIfOriginBlocked, _modelUnit);
                if (fromCell != null)
                {
                    fromPos = fromCell.position;
                }
            }

            // setup the path request
            _pendingPathRequest = new InternalPathRequest
            {
                from = fromPos,
                to = to,
                pathType = type,
                requester = this,
                requesterProperties = _modelUnit,
                pathFinderOptions = _modelUnit.pathFinderOptions
            };

            _stopped = false;
            _lastPathRequestTime = Time.time;
            GameServices.pathService.QueueRequest(_pendingPathRequest, _modelUnit.pathFinderOptions.pathingPriority);
        }

        void INeedPath.ConsumePathResult(PathResult result)
        {
            // Execute on the main thread to avoid multi-threading issues or the need for using a lock or Monitor
            NavLoadBalancer.marshaller.ExecuteOnMainThread(() =>
            {
                // If we have stopped or get back the result of a request other than the one we currently expect, just toss it as it will be outdated.
                if (result.originalRequest != _pendingPathRequest)
                {
                    return;
                }

                _pendingResult = result;
                _pendingPathRequest = null;
            });
        }

        private void ConsumePath()
        {
            PathResult result = _pendingResult;
            _pendingResult = null;

            var req = result.originalRequest as InternalPathRequest;

            //Consume way points if appropriate. This must be done prior to the processing of the result, since if the request was a way point request, the first item in line is the one the result concerns.
            if (req.pathType == InternalPathRequest.Type.Waypoint)
            {
                AnnounceEvent(UnitNavigationEventMessage.Event.WaypointReached, _wayPoints.Dequeue(), null);
            }

            //Process the result
            if (!ProcessAndValidateResult(result))
            {
                return;
            }

            _currentPath = result.path;

            // If we have a formation we want to recalculate positions
            // We turn the model unit to face the new path direction so the formation can be sorted right away.
            if (_currentFormation != null)
            {
                _modelUnit.transform.LookAt(_currentPath[1].position);
                SetFormation(_currentFormation);
            }

            SetVectorFieldPath();
        }

        private void SetManualPath(Path path, ReplanCallback onReplan)
        {
            if (this.count == 0 || _modelUnit == null)
            {
                // if the group has no members, then no need to set anything
                return;
            }

            if (path == null || path.count == 0)
            {
                StopInternal();
                return;
            }

            _manualReplan = onReplan;
            _currentPath = path;
            _lastPathRequestTime = Time.time;

            SetVectorFieldPath();
        }

        /// <summary>
        /// Replans the path - i.e. requests a new path, either through manual replanning or normally.
        /// </summary>
        public void ReplanPath()
        {
            if (_pendingResult != null || _pendingPathRequest != null)
            {
                return;
            }

            if (_currentPath == null || _currentPath.count == 0)
            {
                return;
            }

            var pathToReplan = _currentPath;
            _currentPath = null;

            var lastPathNode = pathToReplan.Last();
            if (_manualReplan != null)
            {
                // if manually replanning, then get the manual replanned path
                var updatedPath = _manualReplan(_modelUnit.gameObject, lastPathNode, pathToReplan);
                SetManualPath(updatedPath, _manualReplan);
            }
            else
            {
                // if not manually replanning, just request a path normally
                RequestPath(lastPathNode.position, InternalPathRequest.Type.Normal);
            }
        }

        private void HandlePathReplan()
        {
            if (_pendingResult != null || _pendingPathRequest != null)
            {
                return;
            }

            //If we are moving entirely off grid, there is no point in replanning, as there is nothing to replan on.
            if (_currentGrid == null || _replanMode == ReplanMode.NoReplan)
            {
                return;
            }

            var now = Time.time;
            if (now - _lastPathRequestTime < _replanInterval)
            {
                // disallow path replanning more often than replanInterval
                return;
            }

            bool replan = true;
            if (_replanMode == ReplanMode.Dynamic)
            {
                // check whether any sections have changed on the group center grid, if the replanMode is dynamic
                replan = _currentGrid.HasSectionsChangedSince(_modelUnit.position, _lastPathRequestTime);
            }

            if (replan)
            {
                ReplanPath();
            }
        }

        private void SetVectorFieldPath()
        {
            _stopped = false;
            this.hasArrived = false;
            _announcedEndOnce = false;

            if (_vectorField != null)
            {
                // if the group has a vector field, set new path on it
                _vectorField.SetNewPath(_currentPath);
            }
            else
            {
                // if the group does not have a vector field - create one
                _vectorField = GameServices.vectorFieldManager.CreateVectorField(this, _currentPath);
            }
        }

        /// <summary>
        /// Adds the specified member to this steering group.
        /// Also initializes model unit-based settings, if they haven't already been initialized.
        /// </summary>
        public override void Add(IUnitFacade member)
        {
            base.Add(member);

            // if the group was created using the 'capacity' ctor overload, we must create model unit when the first unit is added
            if (_modelUnit == null)
            {
                CreateModelUnit();
                InitializeModelUnitSettings();

                _groupCog = member.position;
                _currentGrid = GridManager.instance.GetGrid(_groupCog);
            }
        }

        /// <summary>
        /// Removes the specified member from this steering group.
        /// Also disposes itself if the last member is removed.
        /// </summary>
        public override void Remove(IUnitFacade member)
        {
            base.Remove(member);

            if (this.count == 0)
            {
                // if the last member was just removed, the group should dispose of and cleanup itself
                CleanupGroup();
            }
            else if (_currentFormation != null && _modelUnit.isAlive)
            {
                SetFormation(_currentFormation);
            }
        }

        private bool ProcessAndValidateResult(PathResult result)
        {
            UnitNavigationEventMessage.Event msgEvent = UnitNavigationEventMessage.Event.None;

            switch (result.status)
            {
                case PathingStatus.Complete:
                case PathingStatus.CompletePartial:
                {
                    /* All is good, no more to do */
                    return true;
                }

                case PathingStatus.NoRouteExists:
                case PathingStatus.EndOutsideGrid:
                {
                    msgEvent = UnitNavigationEventMessage.Event.StoppedNoRouteExists;
                    break;
                }

                case PathingStatus.StartOutsideGrid:
                {
                    msgEvent = UnitNavigationEventMessage.Event.StoppedUnitOutsideGrid;
                    break;
                }

                case PathingStatus.DestinationBlocked:
                {
                    msgEvent = UnitNavigationEventMessage.Event.StoppedDestinationBlocked;
                    break;
                }

                case PathingStatus.Decayed:
                {
                    //We cannot reissue the request here, since we may be on a different thread, but then again why would we issue the request again if it had a decay threshold its no longer valid.
                    msgEvent = UnitNavigationEventMessage.Event.StoppedRequestDecayed;
                    break;
                }

                case PathingStatus.Failed:
                {
                    Debug.LogError("Path request failed: " + result.errorInfo);
                    break;
                }
            }

            var destination = result.originalRequest.to;
            var pendingWaypoints = _wayPoints.count > 0 ? _wayPoints.ToArray() : Empty<Vector3>.array;

            Stop();

            if (msgEvent != UnitNavigationEventMessage.Event.None)
            {
                AnnounceEvent(msgEvent, destination, pendingWaypoints);
            }

            return false;
        }

        /// <summary>
        /// Announce a UnitNavigationEventMessage event.
        /// </summary>
        /// <param name="e">The event to announce.</param>
        /// <param name="destination">The current destination.</param>
        /// <param name="pendingWaypoints">The pending waypoints.</param>
        public void AnnounceEvent(UnitNavigationEventMessage.Event e, Vector3 destination, Vector3[] pendingWaypoints)
        {
            _navMessage.isHandled = false;
            _navMessage.eventCode = e;
            _navMessage.destination = destination;
            _navMessage.pendingWaypoints = pendingWaypoints ?? Empty<Vector3>.array;
            GameServices.messageBus.Post(_navMessage);
        }

        private void StopInternal()
        {
            _stopped = true;
            _wayPoints.Clear();
            _currentPath = null;
            _pendingPathRequest = null;
            _pendingResult = null;
            _manualReplan = null;
        }

        private void CleanupGroup()
        {
            // null vector field and everything else
            _vectorField = null;
            StopInternal();

            // remove the group from load balancer
            NavLoadBalancer.steering.Remove(this);

            if (_modelUnit != null)
            {
                // destroy the model unit, clearing it's group to prevent reentry
                _modelUnit.transientGroup = null;
                GameObject.Destroy(_modelUnit.gameObject, 0.1f);
            }
        }

        /// <summary>
        /// Enqueues all passed waypoints to this steering group's existing waypoints (if any).
        /// Used in conjunction with portalling and switching over to a new group - the new group needs to have the same waypoints as the old one did.
        /// </summary>
        public void SetWaypoints(IIterable<Vector3> waypoints)
        {
            int count = waypoints.count;
            for (int i = 0; i < count; i++)
            {
                _wayPoints.Enqueue(waypoints[i]);
            }
        }

        /// <summary>
        /// Sets all associated units' transientGroup reference to this group.
        /// Also handles the creation of the Model Unit game object for group steering purposes.
        /// </summary>
        protected override void PrepareForAction()
        {
            base.PrepareForAction();

            // if the model unit has not been instantiated yet, then do so...
            if (_modelUnit == null && this.count > 0)
            {
                CreateModelUnit();
                InitializeModelUnitSettings();
            }
        }

        private void CreateModelUnit()
        {
            GameObject unit = this[0].gameObject;
            GameObject modelUnitGO = new GameObject("Model Unit");

            //Start out inactive so all components are added before life cycle begins, i.e. Awake, Start etc.
            modelUnitGO.SetActive(false);
            var collider = modelUnitGO.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            // make sure model unit is kinematic and does not respond to Unity Gravity
            var rigidbody = modelUnitGO.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            // now we need to copy and clone all the relevant components from the group's units to the new model unit (group leader)

            // copy and clone SteerableUnitComponent, if it exists and is enabled
            var steerableComponent = unit.GetComponent<SteerableUnitComponent>();
            if (steerableComponent != null && steerableComponent.enabled)
            {
                var newSteerable = modelUnitGO.AddComponent<SteerableUnitComponent>();
                newSteerable.CloneFrom(steerableComponent);
            }

            //copy and clone IHeightNavigator component
            var heightNav = unit.As<IHeightNavigator>();
            if (heightNav != null)
            {
                var newNav = modelUnitGO.AddComponent(heightNav.GetType()) as IHeightNavigator;
                newNav.CloneFrom(heightNav);
            }

            // copy and clone IDefineSpeed component, if it exists and is enabled
            var speedComp = unit.As<IDefineSpeed>();
            if (speedComp != null)
            {
                var newSpeedComp = modelUnitGO.AddComponent(speedComp.GetType()) as IDefineSpeed;
                newSpeedComp.CloneFrom(speedComp);
            }

            // copy and clone PathOptionsComponent, if it exists and is enabled
            var pathOptions = unit.GetComponent<PathOptionsComponent>();
            if (pathOptions != null && pathOptions.enabled)
            {
                var newPathOptions = modelUnitGO.AddComponent<PathOptionsComponent>();
                newPathOptions.CloneFrom(pathOptions);
            }

            // copy and clone SteerToAlignWithVelocity, if it exists and is enabled
            var steerToAlign = unit.GetComponent<SteerToAlignWithVelocity>();
            if (steerToAlign != null && steerToAlign.enabled)
            {
                var newSteerToAlign = modelUnitGO.AddComponent<SteerToAlignWithVelocity>();
                newSteerToAlign.CloneFrom(steerToAlign);
            }

            // copy and cone PathVisualizer, if it exists and is enabled
            var pathVisualizer = unit.GetComponent<PathVisualizer>();
            if (pathVisualizer != null && pathVisualizer.enabled)
            {
                var newPathVisualizer = modelUnitGO.AddComponent<PathVisualizer>();
                newPathVisualizer.CloneFrom(pathVisualizer);
            }

            // copy and clone SteerForPathComponent, if it exists and is enabled
            var steerForPath = unit.GetComponent<SteerForPathComponent>();
            if (steerForPath != null && steerForPath.enabled)
            {
                var newSteerForPath = modelUnitGO.AddComponent<SteerForPathComponent>();
                newSteerForPath.CloneFrom(steerForPath);
            }

            // copy and clone UnitComponent, if it exists and is enabled
            var unitComp = unit.GetComponent<UnitComponent>();
            if (unitComp != null && unitComp.enabled)
            {
                var newUnitComponent = modelUnitGO.AddComponent<UnitComponent>();
                newUnitComponent.CloneFrom(unitComp);
                newUnitComponent.isSelectable = false;
                newUnitComponent.selectionVisual = null;
                newUnitComponent.enabled = true;
            }

            // copy and clone SteerForVectorFieldComponent, if it exists and is enabled
            var steerForVectorField = unit.GetComponent<SteerForVectorFieldComponent>();
            if (steerForVectorField != null && steerForVectorField.enabled)
            {
                var newSteerForVector = modelUnitGO.AddComponent<SteerForVectorFieldComponent>();
                newSteerForVector.CloneFrom(steerForVectorField);
                newSteerForVector.arrivalRadiusMargin = 0f;
                newSteerForVector.arrivalDistance = 0.2f;
            }

            // copy and clone SteeringController, if it exists and is enabled
            var steeringController = unit.GetComponent<SteeringController>();
            if (steeringController != null && steeringController.enabled)
            {
                modelUnitGO.AddComponent<SteeringController>();
            }

            // copy Apex Component Master
            var componentMaster = unit.GetComponent<ApexComponentMaster>();
            if (componentMaster != null && componentMaster.enabled)
            {
                modelUnitGO.AddComponent<ApexComponentMaster>();
            }

            // active the model unit here at last - also make sure a UnitFacade is made and that it knows of this group
            modelUnitGO.SetActive(true);
            _modelUnit = modelUnitGO.GetUnitFacade();
            _modelUnit.transientGroup = this;

            if (nextNodePosition.HasValue)
            {
                // if there is a path and the next node position is valid, then place the model unit at the same position as the nearest unit in the group compared to the next node position
                Vector3 nearestPos = Vector3.zero;
                float shortestDistance = float.MinValue;
                for (int i = 0; i < this.count; i++)
                {
                    Vector3 pos = this[i].position;
                    float distance = (pos - nextNodePosition.Value).sqrMagnitude;
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestPos = pos;
                    }
                }

                _modelUnit.transform.position = nearestPos;
            }
            else
            {
                // if there is no path or the next node position is not valid, just place model unit at the center of the group
                _modelUnit.transform.position = GetGroupCenterOfGravity();
            }

            _modelUnit.transform.forward = this[0].forward;
        }

        /// <summary>
        /// Transfers the formation from an old group to a new one (used only in Portalling).
        /// </summary>
        /// <param name="formation">The formation to transfer.</param>
        /// <param name="expectedCount">The expected group member count.</param>
        /// <param name="oldGroup">The old group to transfer from.</param>
        public void TransferFormation(IFormation formation, int expectedCount, DefaultSteeringTransientUnitGroup oldGroup)
        {
            if (this.count == 0 || expectedCount == 0 || formation == null || oldGroup == null)
            {
                return;
            }

            if (_formationPositions == null)
            {
                // if the formation positions list is null, then make it now
                _formationPositions = new DynamicArray<Vector3>(expectedCount);
            }
            else
            {
                // if the formation positions list is not null, just clear it
                _formationPositions.Clear();
            }

            // set formation reference
            _currentFormation = formation;

            // populate formation positions list with formation positions
            for (int i = 0; i < expectedCount; i++)
            {
                _formationPositions.Add(formation.GetFormationPosition(expectedCount, i, oldGroup[i]));
            }
        }

        /// <summary>
        /// Sets the currently active formation.
        /// </summary>
        /// <param name="formation">The desired formation.</param>
        public override void SetFormation(IFormation formation)
        {
            if (this.count == 0)
            {
                // no group members, exit early
                return;
            }

            if (_formationPositions == null)
            {
                // if the formation positions list is null, then make it now
                _formationPositions = new DynamicArray<Vector3>(count);
            }
            else
            {
                // if the formation positions list is not null, just clear it
                _formationPositions.Clear();
            }

            if (formation == null)
            {
                // if formation passed was null, treat it as a formation clearing
                _currentFormation = null;
                return;
            }

            // set formation reference
            _currentFormation = formation;

            // prepare the group for movement
            PrepareForAction();

            // populate formation positions list with formation positions
            for (int i = 0; i < count; i++)
            {
                this[i].hasArrivedAtDestination = false;
                _formationPositions.Add(formation.GetFormationPosition(count, i, this[i]));
            }

            SetupFormationIndices();
        }

        /// <summary>
        /// Sets up the formation indices, based on the already populated list of formation positions.
        /// </summary>
        protected virtual void SetupFormationIndices()
        {
            if (_formationPositions == null || _formationPositions.count == 0)
            {
                // exit early if we don't have valid formation positions
                return;
            }

            if (_tempFormationPositions == null)
            {
                // prepare temporary formation positions list
                _tempFormationPositions = new List<FormationPositionWithIndex>(count);
            }

            int cogCount = 0;
            Vector3 center = Vector3.zero;
            for (int f = 0; f < count; f++)
            {
                IPositioned formPos = GetFormationPosition(f);
                if (formPos == null)
                {
                    continue;
                }

                // populate list of temporary formation positions along with their index in the list
                _tempFormationPositions.Add(new FormationPositionWithIndex(formPos.position, cogCount));

                cogCount++;
                center += formPos.position;
            }

            // compute center of gravity for all the formation positions
            center /= cogCount;

            //Sort the units by their distance to the group center, in descending order,
            //i.e. those furthest away are first to choose a formation position
            _unitSortComparer.compareTo = center;
            this.Sort(_unitSortComparer);

            // For each unit group member in the group, check all the remaining available formation positions
            // Whatever remaining available formation position is nearest to the unit, the unit picks as its formation position
            for (int i = 0; i < this.count; i++)
            {
                var unit = this[i];
                if (unit == null)
                {
                    continue;
                }

                Vector3 unitPos = unit.position;

                float shortestDistance = float.MaxValue;
                int formationIndex = -1;
                int actualFormationIndex = -1;

                int formationsCount = _tempFormationPositions.Count;
                for (int j = 0; j < formationsCount; j++)
                {
                    IPositioned formPos = _tempFormationPositions[j].position;

                    float distance = (formPos.position - unitPos).sqrMagnitude;
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        formationIndex = j;
                        actualFormationIndex = _tempFormationPositions[j].index;
                    }
                }

                _tempFormationPositions.RemoveAt(formationIndex);
                unit.formationIndex = actualFormationIndex;
            }

            // clear the temporary formation positions list of any possible leftovers (although there shouldn't be any)
            _tempFormationPositions.Clear();
        }

        /// <summary>
        /// Gets the formation position - i.e. the formation positions around the model unit.
        /// </summary>
        /// <param name="formationIndex">Unit's formation index.</param>
        /// <returns>An <see cref="IPositioned"/> formation position, or null in case of error</returns>
        public IPositioned GetFormationPosition(int formationIndex)
        {
            if (_formationPositions == null || _formationPositions.count == 0 || _modelUnit == null || _modelUnit.transform == null)
            {
                // if missing formation positions or model unit, exit early
                return null;
            }

            if (formationIndex < 0 || formationIndex >= _formationPositions.count)
            {
                // if the formation index is invalid, exit early
                return null;
            }

            // return the formation position moved to be relative to the model unit and transformed to match model unit's rotation
            Vector3 formationPos = _formationPositions[formationIndex];
            return new Position(_modelUnit.position + _modelUnit.transform.TransformDirection(formationPos));
        }

        /// <summary>
        /// Gets the final formation position - i.e. the formation positions around the final vector field destination.
        /// </summary>
        /// <param name="formationIndex">Unit's formation index.</param>
        /// <returns>An <see cref="IPositioned"/> formation position, or null in case of error</returns>
        public IPositioned GetFinalFormationPosition(int formationIndex)
        {
            if (_formationPositions == null || _formationPositions.count == 0 || _modelUnit == null || _modelUnit.transform == null)
            {
                // if missing formation positions or model unit, exit early
                return null;
            }

            if (formationIndex < 0 || formationIndex >= _formationPositions.count)
            {
                // if the formation index is invalid, exit early
                return null;
            }

            if (_vectorField == null || _vectorField.destination == null)
            {
                // if no vector field has been registered, exit early
                return null;
            }

            // return the formation position moved to be relative to the model unit and transformed to match model unit's rotation
            Vector3 formationPos = _formationPositions[formationIndex];
            if (!_modelUnit.hasArrivedAtDestination)
            {
                // if the model unit has not arrived, then return the formation positions around the final vector field destination
                return new Position(_vectorField.destination.position + _modelUnit.transform.TransformDirection(formationPos));
            }
            else
            {
                // if the model unit has arrived, then return the formation positions around the model unit, since it is not expected to move anymore
                return new Position(_modelUnit.position + _modelUnit.transform.TransformDirection(formationPos));
            }
        }

        /// <summary>
        /// Convenience method handling the waypoints.
        /// </summary>
        private void HandleWaypoints()
        {
            if (_wayPoints.count == 0 || _pendingResult != null || _pendingPathRequest != null || _vectorField == null || _vectorField.destination == null)
            {
                // if no waypoints, waiting for a result or no detsination is available, don't handle waypoints
                return;
            }

            // prepare local variables
            Vector3 destination = _vectorField.destination.position;
            float requestNextWaypointDistanceSqr = _requestNextWaypointDistance * _requestNextWaypointDistance;

            bool oneUnitIsIsNear = false;
            for (int i = 0; i < this.count; i++)
            {
                var unit = this[i];
                if (unit == null)
                {
                    // discard null units
                    continue;
                }

                if (unit.hasArrivedAtDestination ||
                    (unit.position - destination).sqrMagnitude < requestNextWaypointDistanceSqr)
                {
                    // if just a single unit is within the requestNextWaypointDistance or has actually arrived, break and set to true
                    oneUnitIsIsNear = true;
                    break;
                }
            }

            if (!oneUnitIsIsNear)
            {
                // return if not a single unit has arrived or is within requestNextWaypointDistance
                return;
            }

            // also group center of gravity or model unit should be within requestNextWaypointDistance
            if ((_groupCog - destination).sqrMagnitude < requestNextWaypointDistanceSqr ||
                (_modelUnit.hasArrivedAtDestination || (_modelUnit.position - destination).sqrMagnitude < requestNextWaypointDistanceSqr))
            {
                RequestPath(_wayPoints.Peek(), InternalPathRequest.Type.Waypoint);
            }
        }

        /// <summary>
        /// Executes the update.
        /// </summary>
        /// <param name="deltaTime">The delta time, i.e. the time passed since the last update.</param>
        /// <param name="nextInterval">The time that will pass until the next update.</param>
        /// <returns>
        /// Can return the next interval by which the update should run. To use the default interval return null.
        /// </returns>
        public float? ExecuteUpdate(float deltaTime, float nextInterval)
        {
            if (this.count == 0)
            {
                CleanupGroup();
                return null;
            }

            if (_stopped || _wait || _modelUnit == null || !_modelUnit.isAlive)
            {
                // if we are supposed to be waiting or stopped, don't try to update
                return null;
            }

            _groupCog = GetGroupCenterOfGravity();
            _currentGrid = GridManager.instance.GetGrid(_modelUnit.position);

            HandleWaypoints();

            if (_pendingResult != null)
            {
                // if there is a pending result - consume it now
                ConsumePath();
            }
            else if (!this.hasArrived)
            {
                // if we haven't arrived yet...
                if (_vectorField != null && _vectorField.nextNodePosition != null)
                {
                    Vector3 nextNodePos = _vectorField.nextNodePosition.position;
                    if ((_groupCog - nextNodePos).sqrMagnitude > (_nextNodeDistance * _nextNodeDistance) &&
                        (_modelUnit.position - nextNodePos).sqrMagnitude > (_nextNodeDistance * _nextNodeDistance))
                    {
                        // only check for replanning when not close to the next node
                        HandlePathReplan();
                    }
                }
            }
            else if (_pendingResult == null && _pendingPathRequest == null && _wayPoints.count == 0)
            {
                // if there are no pending results...
                StopInternal();

                if (!_announcedEndOnce)
                {
                    // make sure we only announce the DestinationReached once
                    AnnounceEvent(UnitNavigationEventMessage.Event.DestinationReached, _groupCog, null);
                    _announcedEndOnce = true;
                }
            }

            return null;
        }

        /// <summary>
        /// Private structure for facilitating having formation positions with an associated index in the formation positions array.
        /// </summary>
        private struct FormationPositionWithIndex
        {
            internal IPositioned position;
            internal int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="FormationPositionWithIndex"/> struct.
            /// </summary>
            /// <param name="pos">The position.</param>
            /// <param name="idx">The index.</param>
            internal FormationPositionWithIndex(Vector3 pos, int idx)
            {
                this.position = new Position(pos);
                this.index = idx;
            }
        }

        /// <summary>
        /// Private, internal path request class - to be able to request normal paths or waypoint paths.
        /// <remarks>Should not be used anywhere else, only for internal use!!</remarks>
        /// </summary>
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