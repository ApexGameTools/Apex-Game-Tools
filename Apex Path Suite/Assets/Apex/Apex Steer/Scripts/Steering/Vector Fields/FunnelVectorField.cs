/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.VectorFields
{
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.PathFinding;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// This vector field type is based on funnelling-principles. The vector field will attempt to point in the direction of the path at all vector field cells.
    /// Fast marching is used to make sure that no vectors point towards a blocked cell.
    /// This FunnelVectorField is, like the FullGridVectorField, only computed once (per replanning/move order), and it has less of a performance impact compared to FullGridVectorField.
    /// Supports portals that actually move units in their action (e.g. jump, teleport), i.e. does not support grid stitching portals (connectors).
    /// Supports only 1 grid. If multiple grids are used, the solution is to use CrossGridVectorField.
    /// </summary>
    public class FunnelVectorField : IVectorField, ILoadBalanced
    {
        private float _funnelWidth;
        private float _obstacleStrengthFactor;
        private bool _allowCornerCutting;
        private bool _allowDiagonals;
        private bool _builtInContainment;
        private bool _announceAllNodes;
        private float _updateInterval;

        private bool _onFinalApproach;

        private Path _currentPath;
        private Vector3 _destination;
        private IGrid _grid;
        private IUnitProperties _unitProperties;

        private float _funnelWidthSqr;
        private Dictionary<Vector3, VectorFieldCell> _cellDirsSet;
        private Dictionary<Vector3, PlaneVector> _fastMarchedCellsSet;

        private DynamicArray<Cell> _tempWalkableNeighbours;
        private DynamicArray<Cell> _extraTempWalkableNeighbours;
        private SimpleQueue<Cell> _openSet;

        private int _currentPathStep;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunnelVectorField"/> class.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="path">The path.</param>
        /// <param name="options">The options.</param>
        public FunnelVectorField(TransientGroup<IUnitFacade> group, Path path, VectorFieldOptions options)
        {
            Ensure.ArgumentNotNull(group, "group");
            this.group = group;

            _currentPath = path;

            var modelUnit = group.modelUnit;
            _unitProperties = modelUnit;
            var pathOptions = modelUnit.pathFinderOptions;

            // cache options locally
            _funnelWidth = options.funnelWidth;
            _obstacleStrengthFactor = options.obstacleStrengthFactor;
            _allowCornerCutting = pathOptions.allowCornerCutting;
            _allowDiagonals = !pathOptions.preventDiagonalMoves;
            _announceAllNodes = modelUnit.pathNavigationOptions.announceAllNodes;

            _builtInContainment = options.builtInContainment;
            _updateInterval = options.updateInterval;

            // pre-allocate lists memory
            _openSet = new SimpleQueue<Cell>(31);
            _tempWalkableNeighbours = new DynamicArray<Cell>(8);
            _extraTempWalkableNeighbours = new DynamicArray<Cell>(8);

            _grid = GridManager.instance.GetGrid(group.modelUnit.position);
            if (_grid != null)
            {
                // we allocate half of the grid's size in order to have a bit more allocated memory than we expect to actually use
                int size = Mathf.CeilToInt((_grid.sizeX * _grid.sizeZ) / 2f);

                float minF = 9.99999944E-11f;
                _cellDirsSet = new Dictionary<Vector3, VectorFieldCell>(size, new Vector3EqualityComparer(minF));
                _fastMarchedCellsSet = new Dictionary<Vector3, PlaneVector>(size, new Vector3EqualityComparer(minF));
            }

            _funnelWidthSqr = _funnelWidth * _funnelWidth;
        }

        /// <summary>
        /// Gets a value indicating whether to repeatedly update this entity each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        public bool repeat
        {
            get { return group.count > 0; }
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        public TransientGroup<IUnitFacade> group
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current path.
        /// </summary>
        public Path currentPath
        {
            get { return _currentPath; }
        }

        /// <summary>
        /// Gets the final destination - e.g. the last path node in the path.
        /// </summary>
        public IPositioned destination
        {
            get
            {
                if (_currentPath == null || _currentPath.count == 0)
                {
                    return null;
                }

                return _currentPath.Last();
            }
        }

        /// <summary>
        /// Gets the next node position - e.g. the next path node in the path.
        /// </summary>
        public IPositioned nextNodePosition
        {
            get
            {
                if (_currentPath == null || _currentPath.count == 0)
                {
                    return null;
                }

                return _currentPath.Peek();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this vector field is on final approach.
        /// </summary>
        /// <value>
        /// <c>true</c> if this vector field is on final approach; otherwise, <c>false</c>.
        /// </value>
        public bool isOnFinalApproach
        {
            get { return _onFinalApproach; }
        }

        /// <summary>
        /// Initializes this vector field (called right after being instantiated).
        /// </summary>
        public void Initialize()
        {
            GenerateFunnelVectorField();

            NavLoadBalancer.steering.Add(this, _updateInterval, true);
        }

        private void ResetField()
        {
            _fastMarchedCellsSet.Clear();
            _cellDirsSet.Clear();
        }

        private void GenerateFunnelVectorField()
        {
            // cannot execute vector field if grid is null
            if (_grid == null)
            {
                return;
            }

            // cannot execute vector field without a valid path
            if (_currentPath == null || _currentPath.count == 0)
            {
                return;
            }

            // the order of these method calls is very important, must not be changed
            StartFastMarchingMethod();
            SmoothFields();
            HandlePortals();
        }

        private void StartFastMarchingMethod()
        {
            _destination = _currentPath.Last().position;
            _currentPathStep = _currentPath.count - 1;

            // find the first cell used for traversal - i.e. the path destination
            // note however, that the destination must be the first found portal node in the path if it exists - we must never go past this
            int pathCount = _currentPath.count - 1;
            for (int i = 1; i < pathCount; i++)
            {
                var pathNode = _currentPath[i];
                if (pathNode is IPortalNode)
                {
                    _destination = _currentPath[i - 1].position;
                    _currentPathStep = i;
                    break;
                }
            }

            Cell destinationCell = _grid.GetCell(_destination, true);
            _openSet.Enqueue(destinationCell);

            while (_openSet.count > 0)
            {
                FunnelMethod(_openSet.Dequeue(), destinationCell);
            }
        }

        private void FunnelMethod(Cell currentCell, Cell destinationCell)
        {
            // prepare local variables
            // the previous path node is either the actual previous path node, or in the case of portalling where we might receive a path with only 1 point - the destination - then we use the group's center of gravity in place of the previous path node
            Vector3 cellPos = currentCell.position;
            Vector3 currentPathNode = _currentPath[_currentPathStep].position;
            Vector3 previousPathNode = _currentPathStep > 0 ? _currentPath[_currentPathStep - 1].position : this.group.centerOfGravity;
            
            // get all walkable cell neighbours
            GetWalkableCellNeighbours(currentCell, _tempWalkableNeighbours, !_allowDiagonals);
            int neighboursCount = _tempWalkableNeighbours.count;
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbour = _tempWalkableNeighbours[i];
                Vector3 neighbourPos = neighbour.position;

                // check whether the neighbour cell we are visiting has unwalkable neighbours
                Vector3 directionVector = Vector3.zero;
                bool noUnwalkables = GetCellNeighboursAndUnwalkability(neighbour, _extraTempWalkableNeighbours, !_allowDiagonals);
                if (noUnwalkables && (neighbourPos - destinationCell.position).sqrMagnitude > _funnelWidthSqr)
                {
                    // if cell has no unwalkable neighbours...
                    // Sum up a vector comprised of 3 vectors: 1) fast marching direction (avoid obstacles), 2) goal direction (point to next node path), 3) path direction (point in path's direction)
                    Vector3 fastMarchVector = neighbourPos.DirToXZ(cellPos).normalized;
                    Vector3 goalVector = neighbourPos.DirToXZ(currentPathNode).normalized;
                    Vector3 pathVector = previousPathNode.DirToXZ(currentPathNode).normalized;
                    directionVector = (fastMarchVector + goalVector + pathVector) / 3f;
                }
                else
                {
                    // if this cell has unwalkable neighbours, then just use fast marching direction and multiply the magnitude with the obstacle strength factor
                    directionVector = neighbourPos.DirToXZ(cellPos).normalized * _obstacleStrengthFactor;
                }

                _fastMarchedCellsSet[neighbourPos] = new PlaneVector(directionVector);

                Vector3 closestPos = GetClosestPositionOnLine(previousPathNode, currentPathNode, neighbourPos);
                if ((closestPos - neighbourPos).sqrMagnitude > _funnelWidthSqr)
                {
                    // only add neighbour cells within the defined funnel area around the path
                    continue;
                }

                _openSet.Enqueue(neighbour);
            }

            // check whether we need to move backwards along the path (we start at the destination)
            // Basically, if we reach a cell that is less than one cell size away from our previous path node, then move on to the next path nodes (decrementally)
            if (_currentPathStep > 0 && (previousPathNode - cellPos).sqrMagnitude < (_grid.cellSize * _grid.cellSize))
            {
                if (_currentPath[_currentPathStep - 1] is IPortalNode)
                {
                    // never move past portal nodes, we never want to remove them because we want to force units to go through the portal
                    return;
                }

                _currentPathStep -= 1;
            }
        }

        private void SmoothFields()
        {
            // loop through all the cells that have been fast marched...
            foreach (var pair in _fastMarchedCellsSet)
            {
                Vector3 pos = pair.Key;
                var cell = _grid.GetCell(pos);

                int smoothVectors = 1;
                Vector3 smoothVector = pair.Value;

                // get all cell neighbours, if there are no unwalkable (or missing neighbours, if built-in containment is true)
                bool noUnwalkables = GetCellNeighboursAndUnwalkability(cell, _tempWalkableNeighbours, !_allowDiagonals);
                if (noUnwalkables)
                {
                    // visit all the neighbours and smooth out using their directions
                    int walkableNeighboursCount = _tempWalkableNeighbours.count;
                    for (int i = 0; i < walkableNeighboursCount; i++)
                    {
                        var neighbour = _tempWalkableNeighbours[i];
                        Vector3 neighbourPos = neighbour.position;

                        Vector3 dir = GetFastMarchedCellAtPos(neighbourPos);
                        if (dir.sqrMagnitude == 0f)
                        {
                            // if the neighbour has not been visited, then ignore it
                            continue;
                        }

                        smoothVectors++;
                        smoothVector += dir;
                    }

                    smoothVector /= smoothVectors;
                }

                // set the vector field cell direction
                _cellDirsSet[pos] = new VectorFieldCell(smoothVector);
            }
        }

        private void HandlePortals()
        {
            // loop through all nodes in the path, except the first and last - as they can never be portals
            int pathCount = _currentPath.count - 1;
            for (int i = 1; i < pathCount; i++)
            {
                var node = _currentPath[i] as PortalCell;
                if (node == null)
                {
                    // if the node is not a portal cell, then ignore it
                    continue;
                }

                if (!_grid.Contains(node.position))
                {
                    // if a path node is not contained in this vector field's grid, then ignore it
                    continue;
                }

                // find all the portal neighbour nodes
                int neighbourCount = node.neighbourNodes.Length;
                for (int j = 0; j < neighbourCount; j++)
                {
                    var neighbour = node.neighbourNodes[j];
                    Vector3 neighbourPos = neighbour.position;

                    // all portal neighbour nodes point to the same index in the path, the index where the portal is
                    VectorFieldCell fieldCell;
                    if (_cellDirsSet.TryGetValue(neighbourPos, out fieldCell))
                    {
                        // ignore nodes not in the vector field
                        _cellDirsSet[neighbourPos] = new VectorFieldCell(fieldCell.direction, i);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the closest position on a line from a specified position.
        /// </summary>
        /// <param name="lineStart">The line start.</param>
        /// <param name="lineEnd">The line end.</param>
        /// <param name="position">The position to compare with.</param>
        /// <returns>Returns the input position as projected orthogonally onto the line </returns>
        private Vector3 GetClosestPositionOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 position)
        {
            Vector3 line = (lineEnd - lineStart);
            float lineSqr = line.sqrMagnitude;
            if (lineSqr == 0f)
            {
                // no difference between line start and end, just return one of them
                return lineEnd;
            }

            float product = Vector3.Dot((position - lineStart), line);
            float distance = product / lineSqr;

            if (distance < 0f)
            {
                // position is projected down to before the line starts - so return line start point
                return lineStart;
            }
            else if (distance > 1f)
            {
                // position is projected down to after the line ends - so return line end point
                return lineEnd;
            }

            // return position orthogonally projected down onto the line
            return lineStart + (line * distance);
        }

        /// <summary>
        /// Gets the cell neighbours and unwalkability - used in vector smoothing.
        /// </summary>
        private bool GetCellNeighboursAndUnwalkability(Cell cell, DynamicArray<Cell> walkableCells, bool preventDiagonals)
        {
            if (walkableCells.count != 0)
            {
                // if the list for holding the cells is not empty, make it empty
                walkableCells.Clear();
            }

            int mx = cell.matrixPosX;
            int mz = cell.matrixPosZ;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0)
                    {
                        // cell in the middle is the cell which neighbours we want, thus ignore the center cell
                        continue;
                    }

                    if (x != 0 && z != 0 && preventDiagonals)
                    {
                        // if we are supposed to prevent diagonal moves, then ignore diagonal cell neighbours
                        continue;
                    }

                    var neighbour = _grid.cellMatrix[mx + x, mz + z];
                    if (neighbour != null)
                    {
                        if (neighbour.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(neighbour, _unitProperties))
                        {
                            // if the neighbour is walkable, and the center cell is walkable from this neighbour, then it is of interest
                            walkableCells.Add(neighbour);
                        }
                        else
                        {
                            // if we find just a single neighbour that is blocked, then we return false
                            return false;
                        }
                    }
                    else if (_builtInContainment)
                    {
                        // if there is a missing cell, and we want to use built-in containment, then return false on first missing cell (off-grid)
                        return false;
                    }
                } /* end neighbour traversal on z */
            } /* end neighbour traversal on x */

            return true;
        }

        /// <summary>
        /// Gets the walkable cell neighbours - used in fast marching.
        /// </summary>
        private void GetWalkableCellNeighbours(Cell cell, DynamicArray<Cell> walkableCells, bool preventDiagonals)
        {
            if (walkableCells.count != 0)
            {
                // if the list for holding the cells is not empty, make it empty
                walkableCells.Clear();
            }

            int mx = cell.matrixPosX;
            int mz = cell.matrixPosZ;

            // prepare non-diagonal neighbours in all 4 directions (up, down, left, right)
            var n1 = _grid.cellMatrix[mx - 1, mz];
            var n2 = _grid.cellMatrix[mx, mz - 1];
            var n3 = _grid.cellMatrix[mx + 1, mz];
            var n4 = _grid.cellMatrix[mx, mz + 1];

            bool lw = false, rw = false, uw = false, dw = false;

            if (n1 != null)
            {
                // check whether this neighbour has been fast marched
                lw = _fastMarchedCellsSet.ContainsKey(n1.position);
                if (!lw)
                {
                    // if the cell has not been fast marched...
                    if (n1.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n1, _unitProperties))
                    {
                        // and the cell is walkable, then add it to the list
                        walkableCells.Add(n1);
                        lw = true;
                    }
                }
            }

            if (n2 != null)
            {
                dw = _fastMarchedCellsSet.ContainsKey(n2.position);
                if (!dw)
                {
                    if (n2.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n2, _unitProperties))
                    {
                        walkableCells.Add(n2);
                        dw = true;
                    }
                }
            }

            if (n3 != null)
            {
                rw = _fastMarchedCellsSet.ContainsKey(n3.position);
                if (!rw)
                {
                    if (n3.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n3, _unitProperties))
                    {
                        walkableCells.Add(n3);
                        rw = true;
                    }
                }
            }

            if (n4 != null)
            {
                uw = _fastMarchedCellsSet.ContainsKey(n4.position);
                if (!uw)
                {
                    if (n4.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n4, _unitProperties))
                    {
                        walkableCells.Add(n4);
                        uw = true;
                    }
                }
            }

            if (preventDiagonals)
            {
                return;
            }

            bool urw, drw, dlw, ulw;
            if (_allowCornerCutting)
            {
                // if we allow corner cuttting, then just one of the neighbours need to be free to go diagonally
                urw = uw || rw;
                drw = dw || rw;
                dlw = dw || lw;
                ulw = uw || lw;
            }
            else
            {
                // however, if we don't allow corner cutting, then both neighbours need to be free to allow diagonal neighbour
                urw = uw && rw;
                drw = dw && rw;
                dlw = dw && lw;
                ulw = uw && lw;
            }

            if (dlw)
            {
                var n5 = _grid.cellMatrix[mx - 1, mz - 1];
                if (n5 != null && !_fastMarchedCellsSet.ContainsKey(n5.position))
                {
                    if (n5.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n5, _unitProperties))
                    {
                        walkableCells.Add(n5);
                    }
                }
            }

            if (ulw)
            {
                var n6 = _grid.cellMatrix[mx - 1, mz + 1];
                if (n6 != null && !_fastMarchedCellsSet.ContainsKey(n6.position))
                {
                    if (n6.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n6, _unitProperties))
                    {
                        walkableCells.Add(n6);
                    }
                }
            }

            if (drw)
            {
                var n7 = _grid.cellMatrix[mx + 1, mz - 1];
                if (n7 != null && !_fastMarchedCellsSet.ContainsKey(n7.position))
                {
                    if (n7.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n7, _unitProperties))
                    {
                        walkableCells.Add(n7);
                    }
                }
            }

            if (urw)
            {
                var n8 = _grid.cellMatrix[mx + 1, mz + 1];
                if (n8 != null && !_fastMarchedCellsSet.ContainsKey(n8.position))
                {
                    if (n8.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n8, _unitProperties))
                    {
                        walkableCells.Add(n8);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the fast marched cell at cell center position. Note the parameter must be a grid cell center position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>PlaneVector with the fast marched direction at the given cell center position</returns>
        private PlaneVector GetFastMarchedCellAtPos(Vector3 pos)
        {
            if (_fastMarchedCellsSet == null)
            {
                return default(PlaneVector);
            }

            PlaneVector value;
            _fastMarchedCellsSet.TryGetValue(pos, out value);
            return value;
        }

        /// <summary>
        /// Gets the vector field cell at a cell center position.
        /// Note the value passed as position MUST be a grid cell center.
        /// </summary>
        /// <param name="cell">The grid cell.</param>
        /// <returns>
        /// The vector field cell at the specified position.
        /// </returns>
        public VectorFieldCell GetFieldCellAtPos(IGridCell cell)
        {
            Ensure.ArgumentNotNull(cell, "cell");

            if (_cellDirsSet == null)
            {
                // if we have no dictionary yet, then return default value of vector field cell
                return default(VectorFieldCell);
            }

            // otherwise, return the vector field cell at the specified position
            VectorFieldCell value;
            _cellDirsSet.TryGetValue(cell.position, out value);
            return value;
        }

        /// <summary>
        /// Sets a new path on this vector field.
        /// </summary>
        /// <param name="path">The path to set.</param>
        public void SetNewPath(Path path)
        {
            if (path == null || path.count == 0)
            {
                Debug.LogWarning("Cannot SetNewPath on VectorField, since Path is null or path count is 0");
                return;
            }

            _grid = GridManager.instance.GetGrid(group.modelUnit.position);
            if (_grid != null)
            {
                // make sure to reset the field when we get a new path, otherwise nothing happens (since we don't fast march cells that have a vector)
                ResetField();

                _currentPath = path;
                _currentPathStep = 0;
                _onFinalApproach = false;

                GenerateFunnelVectorField();
            }
        }

        private void PopPathNode()
        {
            // after popping a path node we need to run HandlePortals again to ensure that path portal indices are correct
            var node = _currentPath.Pop();
            HandlePortals();

            if (_announceAllNodes)
            {
                // if we are supposed to announce all nodes that we pop, then do so...
                var g = this.group as DefaultSteeringTransientUnitGroup;
                if (g != null)
                {
                    g.AnnounceEvent(UnitNavigationEventMessage.Event.NodeReached, node.position, null);
                }
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
            if (group == null || group.count == 0)
            {
                return null;
            }

            if (!_onFinalApproach && _currentPath != null && _currentPath.count > 0)
            {
                var modelUnit = group.modelUnit;
                if (modelUnit == null || modelUnit.transform == null)
                {
                    // early exit if group does not yet have a model unit
                    return null;
                }

                // Check for vector field final approach
                Vector3 groupCog = group.centerOfGravity;
                float goalDistance = (groupCog - _destination).sqrMagnitude;
                float modelUnitDistance = (modelUnit.position - _destination).sqrMagnitude;

                // double unit radius to increase robustness of final approach detection
                float radius = modelUnit.radius * 2f;
                float sqrRadius = radius * radius;

                if (goalDistance <= sqrRadius || modelUnitDistance <= sqrRadius)
                {
                    // if we are already near the goal, final approach now
                    _onFinalApproach = true;
                    while (_currentPath.count > 1)
                    {
                        _currentPath.Pop();
                    }

                    return null;
                }

                int pathCount = _currentPath.count - 1;
                for (int i = 0; i < pathCount; i++)
                {
                    if (_currentPath[i] is IPortalNode || _currentPath[i + 1] is IPortalNode)
                    {
                        // never pop portal nodes, we never want to remove them because we want to force units to go through the portal
                        break;
                    }

                    Vector3 pathNode = _currentPath[i].position;
                    float cogDistance = (groupCog - pathNode).sqrMagnitude;
                    float unitDistance = (modelUnit.position - pathNode).sqrMagnitude;
                    if (cogDistance <= sqrRadius || cogDistance > goalDistance || unitDistance <= sqrRadius)
                    {
                        // pop all path nodes that we either have passed, or are passing now
                        PopPathNode();
                        break;
                    }
                }

                // start final approach if there's only one node (the destination) left
                _onFinalApproach = _currentPath.count <= 1;
            }

            return null;
        }

        /// <summary>
        /// A visualization method called in OnDrawGizmos - only used for debugging.
        /// </summary>
        public void DebugRender()
        {
            if (_grid == null)
            {
                return;
            }

            Vector3 extraHeight = new Vector3(0f, 0.01f, 0f);

            foreach (var c in _cellDirsSet)
            {
                Vector3 pos = c.Key + extraHeight;
                VectorFieldCell fieldCell = c.Value;

                if (fieldCell.pathPortalIndex == -1)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }

                Gizmos.DrawSphere(pos, 0.25f);
                Vector3 fieldCellDirection = fieldCell.direction.normalized;
                Gizmos.DrawLine(pos, pos + fieldCellDirection);
            }
        }
    }
}