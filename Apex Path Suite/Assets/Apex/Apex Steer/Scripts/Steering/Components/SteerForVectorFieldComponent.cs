/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using System;
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.Services;
    using Apex.Steering.VectorFields;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to follow a <see cref="IVectorField"/>, including escaping when in a blocked cell and requesting a path if completely stuck.
    /// Also handles portalling and initial group arrival.
    /// Requires the DefaultSteeringTransientUnitGroup (or something derived from it).
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Vector Field", 1031)]
    [ApexComponent("Steering")]
    public class SteerForVectorFieldComponent : ArrivalBase
    {
        /// <summary>
        /// This radius margin is added to the unit's radius and used to evaluate whether hasArrivedAtDestination is true, i.e. when the distance to the destination is less than the radius plus this margin.
        /// </summary>
        [MinCheck(0.1f, label = "Arrival Radius Margin", tooltip = "This radius margin is added to the unit's radius and used to evaluate whether hasArrivedAtDestination is true, i.e. when the distance to the destination is less than the radius plus this margin.")]
        public float arrivalRadiusMargin = 2f;

        private Vector3 _endOfResolvedPath;
        private float _remainingSquaredDistance;
        private Vector3 _nextPoint;
        private Path _currentPath;
        private bool _stopped;
        private bool _isPortalling;
        private DefaultSteeringTransientUnitGroup _nextGroup;
        private int _lastPathPortalIndex;
        private DynamicArray<Cell> _neighbours;
        private bool _requestedPath;
        private bool _enabledSoloPath;

        private SteeringController _steeringController;
        private DistanceComparer<Cell> _cellComparer = new DistanceComparer<Cell>(true);

        /// <summary>
        /// Called on Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            // pre-allocate list memory
            _neighbours = new DynamicArray<Cell>(8);
            _lastPathPortalIndex = -1;

            var unit = this.GetUnitFacade();
            if (!unit.isMovable)
            {
                // if this unit is not Movable, it cannot and will not request a path if it gets stuck outside the vector field
                Debug.LogWarning(this.gameObject.name + " does not have a component that implements IMovable, which means that if it gets stuck, it cannot request a path to get to the destination");
            }

            _steeringController = this.GetComponent<SteeringController>();
            if (_steeringController == null)
            {
                Debug.LogWarning(this.gameObject.name + " does not have a SteeringController component, which means that it cannot disable formations while solo pathing");
            }
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (_isPortalling)
            {
                // if portalling, pause the output and exit early
                output.pause = true;
                return;
            }

            if (_requestedPath)
            {
                // if waiting for a path out of a tight spot, then just wait for it to complete
                output.pause = true;
                return;
            }

            var unit = input.unit;
            var group = unit.transientGroup as DefaultSteeringTransientUnitGroup;
            if (group == null || group.count == 0)
            {
                // if the group type is not DefaultSteeringTransientUnitGroup (or a derived type) return
                return;
            }

            var grid = input.grid;
            if (grid == null)
            {
                // disable steer for vector field while off grid
                return;
            }

            var vectorField = group.vectorField;
            if (vectorField == null)
            {
                // if there is no vector field, exit
                return;
            }

            Path currentPath = vectorField.currentPath;
            if (currentPath == null || currentPath.count == 0)
            {
                // if the vector field has no path, exit
                return;
            }

            Vector3 selfPos = unit.position;
            var selfCell = grid.GetCell(selfPos);
            if (selfCell == null)
            {
                // if there is no cell beneath this unit, exit early
                return;
            }

            if (!object.ReferenceEquals(currentPath, _currentPath))
            {
                // Vector field has gotten a new path
                _remainingSquaredDistance = currentPath.CalculateSquaredLength();
                _endOfResolvedPath = currentPath.Last().position;
                _stopped = false;
                _lastPathPortalIndex = -1;
                _currentPath = currentPath;
                unit.hasArrivedAtDestination = false;

                if (_enabledSoloPath)
                {
                    // we get a new group path while being on the solo, then stop solo pathing
                    EndSoloPath();
                }
            }

            if (_stopped || unit.hasArrivedAtDestination)
            {
                // if this unit has not stopped, it should stop
                return;
            }

            // get the vector field cell - note that the position passed must be a cell center position
            var vectorFieldCell = vectorField.GetFieldCellAtPos(selfCell);
            Vector3 vectorFieldVector = vectorFieldCell.direction;
            if (vectorFieldVector.sqrMagnitude == 0f)
            {
                // if the cell this unit is standing on has not been visited
                if (!_enabledSoloPath)
                {
                    // if no vector field vector for the current cell exists - and we are not currently on a solo path - handle it
                    HandleMissingVectorFromField(selfCell, vectorField, input, output);
                }
                else
                {
                    // if we are on a solo path, and the vector field vector is still missing, then speed up to max speed
                    output.maxAllowedSpeed = unit.maximumSpeed;
                }

                // don't proceed if missing a vector field cell
                return;
            }

            int pathPortalIndex = vectorFieldCell.pathPortalIndex;
            if (pathPortalIndex != -1 && pathPortalIndex != _lastPathPortalIndex)
            {
                // if we are on a portal cell that is different from the last portal
                _lastPathPortalIndex = pathPortalIndex;
                HandlePortal(unit, group, vectorField, pathPortalIndex, input.grid);
                return;
            }

            Vector3 nextNodePosition = vectorField.nextNodePosition.position;
            if (_nextPoint != nextNodePosition)
            {
                // Vector field has switched to the next point in the path
                _remainingSquaredDistance -= (_nextPoint - nextNodePosition).sqrMagnitude;
                _stopped = false;
                _nextPoint = nextNodePosition;
            }

            if (_enabledSoloPath)
            {
                // there is a valid vector field with valid vectors - so stop solo pathing
                EndSoloPath();
            }

            var modelUnit = group.modelUnit;
            Vector3 steeringVector = Vector3.zero;
            Vector3 unitVectorFieldVector = selfPos + vectorFieldVector;
            if (object.ReferenceEquals(unit, modelUnit))
            {
                // run arrival for model unit
                float slowingDistanceSqr = this.slowingDistance * this.slowingDistance;
                Vector3 dirEnd = selfPos.DirToXZ(_endOfResolvedPath);
                if ((dirEnd.sqrMagnitude < slowingDistanceSqr) &&
                   (_remainingSquaredDistance < slowingDistanceSqr || vectorField.isOnFinalApproach))
                {
                    // arrive normally (I am the first in my group)
                    steeringVector = Arrive(nextNodePosition, input);
                    if (this.hasArrived)
                    {
                        // inform the group that it has arrived and stop
                        group.hasArrived = true;
                        _stopped = true;
                        unit.hasArrivedAtDestination = true;
                    }
                }
                else
                {
                    steeringVector = SeekWithVectorFieldVector(unitVectorFieldVector, input);
                }
            }
            else
            {
                // evaluate whether we have arrived at/near the final destination
                float rad = unit.radius + this.arrivalDistance + arrivalRadiusMargin;
                var finalFormPos = group.GetFinalFormationPosition(unit.formationIndex);
                Vector3 arrivalPos = finalFormPos != null && unit.formationPos != null ? finalFormPos.position : vectorField.destination.position;
                if (arrivalPos.DirToXZ(selfPos).sqrMagnitude < (rad * rad))
                {
                    unit.hasArrivedAtDestination = true;
                    return;
                }

                // if not model unit, just seek in direction of the vector field vector
                steeringVector = SeekWithVectorFieldVector(unitVectorFieldVector, input);
            }

            if (steeringVector.sqrMagnitude == 0f)
            {
                return;
            }

            output.desiredAcceleration = steeringVector;
        }

        /// <summary>
        /// Convenience method for seeking towards a position while also calculating the required slowing distance
        /// </summary>
        /// <param name="to">The target seek position</param>
        /// <param name="input">The steering input.</param>
        /// <returns>A seek vector in the direction towards the target seek position 'to' from this unit.</returns>
        private Vector3 SeekWithVectorFieldVector(Vector3 to, SteeringInput input)
        {
            // not arriving yet, but calculate slowing distance
            if (this.autoCalculateSlowingDistance)
            {
                CalculateRequiredSlowingDistance(input);
            }

            // Seek in the direction of the vector field cell
            return Seek(to, input);
        }

        /// <summary>
        /// Handles portalling.
        /// </summary>
        /// <param name="unitData">This unit's UnitFacade.</param>
        /// <param name="group">This unit's current/old group.</param>
        /// <param name="vectorField">The vector field.</param>
        /// <param name="pathPortalIndex">Index of the portal in the current path.</param>
        /// <param name="grid">The grid.</param>
        private void HandlePortal(IUnitFacade unitData, DefaultSteeringTransientUnitGroup group, IVectorField vectorField, int pathPortalIndex, IGrid grid)
        {
            var portal = _currentPath[pathPortalIndex] as IPortalNode;
            if (portal == null)
            {
                // if the path node that was reported as a portal turns out not to be - return
                return;
            }

            if (object.ReferenceEquals(unitData, group.modelUnit))
            {
                // don't ever let model unit jump portal
                return;
            }

            int groupCount = group.count;
            var to = _currentPath[pathPortalIndex + 1];

            // we consider a portal a "far portal" when the distance between it and its partner is more than the diagonal cell size
            // 'far portal' means that it is NOT considered a grid stitching connector portal
            // Requires that grid stitching portals are always placed adjacent to each other
            float portalNewGroupThreshold = (grid.cellSize * Consts.SquareRootTwo) + 0.1f;
            bool isFarPortal = (portal.position - portal.partner.position).sqrMagnitude > (portalNewGroupThreshold * portalNewGroupThreshold);

            if (isFarPortal)
            {
                // if it is a far portal, we need to make or use the next group
                if (group.nextGroup == null)
                {
                    // new group does not exist yet, so create it and tell the old group about it
                    var groupStrat = GroupingManager.GetGroupingStrategy<IUnitFacade>();
                    if (groupStrat == null)
                    {
                        Debug.Log("No Grouping Strategy has been registered for IUnitFacade");
                        return;
                    }

                    var newGroup = groupStrat.CreateGroup(groupCount) as DefaultSteeringTransientUnitGroup;
                    if (newGroup == null)
                    {
                        return;
                    }

                    group.nextGroup = newGroup;
                    _nextGroup = newGroup;
                }
                else
                {
                    // new group exists, so just use it
                    _nextGroup = group.nextGroup;
                }

                // make sure to remove the unit from the old group
                group.Remove(unitData);
            }

            _isPortalling = true;
            // actually execute the portal
            portal.Execute(
                unitData.transform,
                to,
                () =>
                {
                    _isPortalling = false;

                    if (isFarPortal)
                    {
                        // if it is a far portal, we are supposed to join up with the new group
                        _nextGroup.Add(unitData);
                        unitData.transientGroup = _nextGroup;

                        if (_nextGroup.count == 1)
                        {
                            // let the first unit in the new group be responsible for setting the new group up
                            if (vectorField.destination != null)
                            {
                                // the new group's path starts on the other side of the portal...
                                int pathCount = _currentPath.count;
                                var newPath = new Path(pathCount - (pathPortalIndex + 2));
                                for (int i = pathCount - 1; i >= pathPortalIndex + 2; i--)
                                {
                                    newPath.Push(_currentPath[i]);
                                }

                                // the first member that joins the new group tells the new group to move along the path of the old group
                                Vector3 destination = vectorField.destination.position;
                                if ((to.position - destination).sqrMagnitude > 1f)
                                {
                                    // check though that the destination is not the same as the starting position
                                    _nextGroup.MoveAlong(newPath);
                                }

                                // pass along old group's waypoints to new group
                                if (group.currentWaypoints.count > 0)
                                {
                                    _nextGroup.SetWaypoints(group.currentWaypoints);
                                }

                                // pass along old group's formation to the new group
                                if (group.currentFormation != null)
                                {
                                    _nextGroup.TransferFormation(group.currentFormation, groupCount, group);
                                }
                            }
                        }
                    }
                });
        }

        /// <summary>
        /// Convenience method for starting the solo pathing.
        /// </summary>
        private void StartSoloPath()
        {
            _steeringController.StartSoloPath();
            _enabledSoloPath = true;
        }

        /// <summary>
        /// Convenience method for ending the solo pathing.
        /// </summary>
        private void EndSoloPath()
        {
            _steeringController.EndSoloPath();
            _enabledSoloPath = false;
        }

        /// <summary>
        /// Handles the situation where there is a missing vector from the vector field.
        /// </summary>
        /// <param name="selfCell">This unit's current cell.</param>
        /// <param name="vectorField">The vector field.</param>
        /// <param name="input">The steering input.</param>
        /// <param name="output">The steering output.</param>
        private void HandleMissingVectorFromField(Cell selfCell, IVectorField vectorField, SteeringInput input, SteeringOutput output)
        {
            var unit = input.unit;
            Vector3 unitPos = unit.position;

            // find all (up to) 8 neighbours
            _neighbours.Clear();
            input.grid.GetConcentricNeighbours(selfCell, 1, _neighbours);

            // sort the neighbours depending on their distance to the unit, i.e. nearest cell neighbours first
            _cellComparer.compareTo = unitPos;
            _neighbours.Sort(_cellComparer);

            // loop through cell neighbours and try to escape to the nearest one that is walkable and has a vector field cell
            int neighboursCount = _neighbours.count;
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbour = _neighbours[i];
                Vector3 neighbourPos = neighbour.position;
                if (neighbour.IsWalkableWithClearance(unit) && vectorField.GetFieldCellAtPos(neighbour).direction.sqrMagnitude != 0f && neighbourPos.y <= unitPos.y)
                {
                    // if the neighbour cell is walkable, has a vector field vector and is at a lower or same height as the unit
                    output.maxAllowedSpeed = unit.maximumSpeed;
                    output.desiredAcceleration = Seek(neighbourPos, input);
                    return;
                }
            }

            // only request path if we can't find a neighbouring cell to escape to, if there is a valid destination and if the unit is actually movable
            if (unit.isMovable && vectorField.destination != null)
            {
                RequestPath(vectorField.destination.position, input);
            }
        }

        /// <summary>
        /// Requests a path.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="input">The steering input.</param>
        private void RequestPath(Vector3 destination, SteeringInput input)
        {
            var unit = input.unit;

            // Request a path normally from the unit's current position
            Action<PathResult> callback = (result) =>
            {
                //TODO: handle partial completion
                if (result.status == PathingStatus.Complete)
                {
                    FollowPath(result.path, unit);
                    return;
                }

                PathRequestCallback(result, unit, unit.position, destination, input.grid);
            };

            QueuePathRequest(unit.position, destination, unit, callback);
        }

        /// <summary>
        /// The path request callback method.
        /// </summary>
        /// <param name="result">The path result.</param>
        /// <param name="unit">This unit's UnitFacade.</param>
        /// <param name="unitPos">This unit's position.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="grid">The grid.</param>
        private void PathRequestCallback(PathResult result, IUnitFacade unit, Vector3 unitPos, Vector3 destination, IGrid grid)
        {
            // if the normal path request fails...
            Action<PathResult> callback = (secondResult) =>
            {
                //TODO: handle partial completion
                if (secondResult.status == PathingStatus.Complete)
                {
                    // add the unit's current position to the path
                    var path = result.path;
                    path.Push(unit); //TODO: this seems fragile, better to add an actual position to avoid storing a ref to the unit.

                    FollowPath(path, unit);
                    return;
                }

                // if the second path request fails, there is nothing to do
                _stopped = true;
            };

            // try to request a path from the nearest (from) walkable cell
            Cell fromCell = grid.GetNearestWalkableCell(unitPos, unitPos, true, 1, unit);
            if (fromCell != null)
            {
                QueuePathRequest(fromCell.position, destination, unit, callback);
                return;
            }

            // if that fails, search through all 8 neighbour cells and attempt to request a path from the nearest IsWalkable neighbour
            int neighboursCount = _neighbours.count;
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbourCell = _neighbours[i];
                if (neighbourCell.IsWalkableWithClearance(unit))
                {
                    QueuePathRequest(neighbourCell.position, destination, unit, callback);
                    return;
                }
            }
        }

        /// <summary>
        /// Convenience method for queuing up path requests.
        /// </summary>
        /// <param name="fromPos">From position.</param>
        /// <param name="toPos">To position.</param>
        /// <param name="unitData">The unit UnitFacade.</param>
        /// <param name="callback">The path request callback.</param>
        private void QueuePathRequest(Vector3 fromPos, Vector3 toPos, IUnitFacade unitData, Action<PathResult> callback)
        {
            _requestedPath = true;

            // request a path...
            var req = new CallbackPathRequest(callback)
            {
                from = fromPos,
                to = toPos,
                requesterProperties = unitData,
                pathFinderOptions = unitData.pathFinderOptions,
            };

            GameServices.pathService.QueueRequest(req, unitData.pathFinderOptions.pathingPriority);
        }

        /// <summary>
        /// Convenience method for telling this unit to start solo pathing along a given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="unit">The unit.</param>
        private void FollowPath(Path path, IUnitFacade unit)
        {
            unit.MoveAlong(path);
            _requestedPath = false;

            StartSoloPath();
        }

        /// <summary>
        /// Clones all properties from another <see cref="SteerForVectorFieldComponent"/>.
        /// </summary>
        /// <param name="steerForVector">The component to clone from.</param>
        public void CloneFrom(SteerForVectorFieldComponent steerForVector)
        {
            this.priority = steerForVector.priority;
            this.weight = steerForVector.weight;

            this.slowingAlgorithm = steerForVector.slowingAlgorithm;
            this.slowingDistance = steerForVector.slowingDistance;
            this.arrivalDistance = steerForVector.arrivalDistance;
            this.autoCalculateSlowingDistance = steerForVector.autoCalculateSlowingDistance;

            this.arrivalRadiusMargin = steerForVector.arrivalRadiusMargin;
        }

        /// <summary>
        /// Stops the unit.
        /// </summary>
        public override void Stop()
        {
            _stopped = true;
        }
    }
}