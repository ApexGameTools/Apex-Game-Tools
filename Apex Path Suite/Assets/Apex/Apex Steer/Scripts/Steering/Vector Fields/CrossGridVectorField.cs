/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.VectorFields
{
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.PathFinding;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A vector field type that is able to handle cross-grid movement.
    /// Performs worse than progressive vector field, but supports cross-grid movement.
    /// Requires the cell size of all grids to be equal, but it can handle differently sized grids.
    /// </summary>
    public class CrossGridVectorField : IVectorField, ILoadBalanced
    {
        private float _obstacleStrengthFactor;
        private bool _allowCornerCutting;
        private bool _allowDiagonals;
        private bool _builtInContainment;
        private bool _announceAllNodes;
        private float _expectedGroupGrowthFactor;
        private float _updateInterval;
        private float _paddingIncrease;
        private float _maxExtraPadding;
        private float _boundsPadding;
        private float _boundsRecalculateThreshold;

        private DynamicArray<Cell> _tempWalkableNeighbours;
        private DynamicArray<Cell> _extraTempWalkableNeighbours;
        private SimpleQueue<Cell> _openSet;

        private Path _currentPath;
        private Vector3 _destination;
        private IList<IGrid> _grids;
        private IUnitProperties _unitProperties;

        private float _portalNewGroupThreshold;
        private int _currentPathStep;
        private float _currentLineSegment;
        private float _extraPadding;
        private RectangleXZ _groupBounds;

        private Vector3 _lastGroupCogCell;

        private int _lastAllocatedSize;
        private Dictionary<Vector3, PlaneVector> _fastMarchedCellsSet;
        private Dictionary<Vector3, VectorFieldCell> _cellDirsSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossGridVectorField"/> class.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="path">The path.</param>
        /// <param name="options">The options.</param>
        public CrossGridVectorField(TransientGroup<IUnitFacade> group, Path path, VectorFieldOptions options)
        {
            Ensure.ArgumentNotNull(group, "group");
            this.group = group;

            _currentPath = path;

            var modelUnit = group.modelUnit;
            _unitProperties = modelUnit;
            var pathOptions = modelUnit.pathFinderOptions;

            // cache options locally
            _allowCornerCutting = pathOptions.allowCornerCutting;
            _allowDiagonals = !pathOptions.preventDiagonalMoves;
            _announceAllNodes = modelUnit.pathNavigationOptions.announceAllNodes;

            _obstacleStrengthFactor = options.obstacleStrengthFactor;
            _builtInContainment = options.builtInContainment;
            _updateInterval = options.updateInterval;
            _paddingIncrease = options.paddingIncrease;
            _maxExtraPadding = options.maxExtraPadding;
            _boundsPadding = options.boundsPadding;
            _boundsRecalculateThreshold = options.boundsRecalculateThreshold;
            _expectedGroupGrowthFactor = 1f + options.expectedGroupGrowthFactor;

            // pre-allocate lists
            _openSet = new SimpleQueue<Cell>(31);
            _tempWalkableNeighbours = new DynamicArray<Cell>(8);
            _extraTempWalkableNeighbours = new DynamicArray<Cell>(8);
            _groupBounds = new RectangleXZ(group.centerOfGravity, _boundsPadding, _boundsPadding);

            _grids = new List<IGrid>(4);
        }

        /// <summary>
        /// Gets the transient unit group.
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
        /// Gets a value indicating whether to repeatedly update this vector field each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the vector field should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        public bool repeat
        {
            get { return group.count > 0; }
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
                if (_currentPath == null || _currentPathStep >= _currentPath.count)
                {
                    return null;
                }

                return _currentPath[_currentPathStep];
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
            get { return _currentPathStep + 1 >= _currentPath.count; }
        }

        /// <summary>
        /// Initializes this vector field (called right after being instantiated).
        /// </summary>
        public void Initialize()
        {
            UpdateCrossGridVectorField(true);

            NavLoadBalancer.steering.Add(this, _updateInterval, true);
        }

        private bool ConstructGroupBounds(bool forceConstruction = false)
        {
            int membersCount = group.count;
            if (membersCount == 0)
            {
                return false;
            }

            // find minimum and maximum coordinates for the group
            float minX = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxZ = float.MinValue;
            Vector3 groupCog = Vector3.zero;
            for (int i = 0; i < membersCount; i++)
            {
                var member = group[i];
                if (member == null)
                {
                    continue;
                }

                var pos = member.position;
                if (pos.x < minX)
                {
                    minX = pos.x;
                }

                if (pos.z < minZ)
                {
                    minZ = pos.z;
                }

                if (pos.x > maxX)
                {
                    maxX = pos.x;
                }

                if (pos.z > maxZ)
                {
                    maxZ = pos.z;
                }

                // sum up all unit positions
                groupCog += pos;
            }

            // divide the summed up unit positions by the unit count, in order to get the "average group position" or center of gravity
            groupCog /= membersCount;
            if (!forceConstruction && ((groupCog - _groupBounds.center).sqrMagnitude < (_boundsRecalculateThreshold * _boundsRecalculateThreshold)))
            {
                // if not forcing the construction, check whether the group has moved from its last group bounds center enough
                return false;
            }

            // first grid is the one where the group center is
            var grid = GridManager.instance.GetGrid(groupCog);
            if (grid == null)
            {
                return false;
            }

            if (_portalNewGroupThreshold <= 0f)
            {
                // if we have not set our portal new group threshold yet, then do it (based on diagonal cell size)
                _portalNewGroupThreshold = (grid.cellSize * Consts.SquareRootTwo) + 0.1f;
            }

            _grids.Clear();
            AddGrid(grid);

            // push the minimum and maximum values by the bounds padding and the potential extra padding resultant from growing bounds
            var padding = _boundsPadding + _extraPadding;
            minX -= padding;
            minZ -= padding;
            maxX += padding;
            maxZ += padding;

            float yPos = groupCog.y;

            // find minimum and maximum cells, in order to construct the group bounds "cell-aligned" and in order to add all relevant grids
            Vector3 vecMinXZ = new Vector3(minX, yPos, minZ);
            var gridMinXZ = GridManager.instance.GetGrid(vecMinXZ);

            // bottom left corner grid
            if (gridMinXZ != null)
            {
                AddGrid(gridMinXZ);

                Cell minXZ = gridMinXZ.GetCell(vecMinXZ, true);
                minX = minXZ.position.x;
                minZ = minXZ.position.z;
            }

            Vector3 vecMaxXZ = new Vector3(maxX, yPos, maxZ);
            var gridMaxXZ = GridManager.instance.GetGrid(vecMaxXZ);

            // top right corner grid
            if (gridMaxXZ != null)
            {
                AddGrid(gridMaxXZ);

                Cell maxXZ = gridMaxXZ.GetCell(vecMaxXZ, true);
                maxX = maxXZ.position.x;
                maxZ = maxXZ.position.z;
            }

            // top left corner grid
            var gridMinXMaxZ = GridManager.instance.GetGrid(new Vector3(minX, yPos, maxZ));
            AddGrid(gridMinXMaxZ);

            // bottom right corner grid
            var gridMaxXMinZ = GridManager.instance.GetGrid(new Vector3(maxX, yPos, minZ));
            AddGrid(gridMaxXMinZ);

            // finally, actually instantiate the new group bounds
            _groupBounds = new RectangleXZ(minX, minZ, (maxX - minX), (maxZ - minZ));

            return true;
        }

        private void FindTemporaryDestination()
        {
            bool isFarDistanceNode = false;

            float cellSize = _grids[0].cellSize;
            float portalThresholdSqr = _portalNewGroupThreshold * _portalNewGroupThreshold;

            // Finds a temporary destination along the path to the final destination
            // the design idea is to find the first node in the path that is outside the group bounds
            _destination = _currentPath[_currentPathStep].position;
            while (_groupBounds.Contains(_destination) && !isOnFinalApproach)
            {
                var node = _currentPath[_currentPathStep + 1];
                var portal = node as IPortalNode;
                if (portal != null)
                {
                    // if the node is a portal node, we need to handle it in two different ways
                    var newNode = _currentPath[_currentPathStep + 2];
                    float portalDistance = (portal.position - portal.partner.position).sqrMagnitude;

                    if (portalDistance > portalThresholdSqr)
                    {
                        // the distance between the portals from and to is more than one cell, thus this is a teleport or jump portal (?)
                        isFarDistanceNode = true;
                        break;
                    }
                    else
                    {
                        // the distance is less than the threshold, so it must be a connector portal (for portal stitching)
                        _currentPathStep++;
                        node = newNode;
                    }
                }

                if (_announceAllNodes)
                {
                    // if we are supposed to announce all nodes, then do so
                    var g = this.group as DefaultSteeringTransientUnitGroup;
                    if (g != null)
                    {
                        g.AnnounceEvent(Apex.Messages.UnitNavigationEventMessage.Event.NodeReached, _destination, null);
                    }
                }

                _currentPathStep++;
                _destination = node.position;
                _currentLineSegment = 0f;
            }

            if (!isFarDistanceNode)
            {
                // now, after finding the first node in the path outside of the group bounds
                // the next idea is to compute a point on the path between the last path node and the current path node
                // only push forward the destination along the path, if we are not supposed to be jumping/teleporting elsewhere
                do
                {
                    if (_currentPathStep == 0)
                    {
                        // if we are on the first node in the path, don't try to go back - since there won't be any predecessors
                        break;
                    }

                    // check whether the previous node is a portal, in which case we need to look one path node further back
                    int lookBack = _currentPathStep - 1;
                    if (_currentPath[lookBack] is IPortalNode)
                    {
                        lookBack--;
                    }

                    // compute the line from the previous path node to the current path node
                    Vector3 lineStart = _currentPath[lookBack].position;
                    Vector3 lineEnd = _currentPath[_currentPathStep].position;
                    Vector3 line = (lineEnd - lineStart);
                    if (line.sqrMagnitude == 0f)
                    {
                        // if there is no distance between the previous point and the current one, then just stop here
                        break;
                    }

                    // compute a point on the line between path nodes...
                    Vector3 tempDest = lineStart + (line.normalized * _currentLineSegment);
                    if ((tempDest - lineStart).sqrMagnitude > line.sqrMagnitude)
                    {
                        // make sure the point on the line never surpasses the current path node
                        break;
                    }

                    // keep pushing the temporary destination forward along the path as long as it is contained in the group bounds
                    if (!_groupBounds.Contains(tempDest))
                    {
                        break;
                    }

                    _destination = tempDest;

                    // push the current point along the line forward by 1 cell size
                    _currentLineSegment += cellSize;
                }
                while (true);
            }

            // make sure the grid where the temporary destination is, is added
            AddGrid(GridManager.instance.GetGrid(_destination));
        }

        private void AllocateFields()
        {
            // figure out the total necessary size that we need to allocate memory-wise
            int totalSize = Mathf.CeilToInt(_groupBounds.size.x * _groupBounds.size.z * _expectedGroupGrowthFactor);
            if ((_fastMarchedCellsSet == null || _cellDirsSet == null) || totalSize > _lastAllocatedSize)
            {
                // if the size needed is more than the last one we allocated - or this is the first time, then allocate
                _lastAllocatedSize = totalSize;

                float minF = 9.99999944E-11f;
                _fastMarchedCellsSet = new Dictionary<Vector3, PlaneVector>(totalSize, new Vector3EqualityComparer(minF));
                _cellDirsSet = new Dictionary<Vector3, VectorFieldCell>(totalSize, new Vector3EqualityComparer(minF));
            }
            else
            {
                // if we don't need to allocate a different amount of memory, then just clear the dictionaries
                _fastMarchedCellsSet.Clear();
                _cellDirsSet.Clear();
            }
        }

        private void UpdateCrossGridVectorField(bool forceBoundsConstruction)
        {
            _lastGroupCogCell = Vector3.zero;

            if (group == null || group.count == 0)
            {
                return;
            }

            if (_currentPath == null || _currentPath.count == 0)
            {
                return;
            }

            // find the grid where the group center is
            Vector3 groupCog = group.centerOfGravity;
            var cogGrid = GridManager.instance.GetGrid(groupCog);
            if (cogGrid == null)
            {
                return;
            }

            // try to find a walkable cell near the group center of gravity
            var groupCogCell = cogGrid.GetNearestWalkableCell(groupCog, groupCog, false, Mathf.CeilToInt((_boundsPadding + _extraPadding) / 2f), _unitProperties);
            if (groupCogCell == null)
            {
                return;
            }

            _lastGroupCogCell = groupCogCell.position;

            if (!ConstructGroupBounds(forceBoundsConstruction))
            {
                // stop executing if we haven't moved enough, i.e. only update the vector field when we move more than recalculate bounds threshold
                return;
            }

            // The order of methods here is very important. They must be called in exactly this order.
            FindTemporaryDestination();
            AllocateFields();
            StartFastMarchingMethod();
            if (GrowOrShrinkBounds())
            {
                // if we are growing the group bounds, there is no point in executing the rest of the code
                return;
            }

            SmoothFields();
            HandlePortals();
        }

        private bool GrowOrShrinkBounds()
        {
            if (_lastGroupCogCell.sqrMagnitude == 0f)
            {
                // if there is no walkable cell at the group center of gravity, then never grow the bounds
                return false;
            }

            if (GetFastMarchedCellAtPos(_lastGroupCogCell).sqrMagnitude == 0f)
            {
                // if the fast marched cells at the group center have not been traversed, we should grow our bounds to try to ensure that we can traverse all the way to the group center
                if (_extraPadding + _paddingIncrease < _maxExtraPadding)
                {
                    _extraPadding += _paddingIncrease;
                    UpdateCrossGridVectorField(true);
                }

                return true;
            }
            else if (_extraPadding >= _paddingIncrease)
            {
                // group bounds has grown, but is not needed as big anymore
                _extraPadding -= _paddingIncrease;
            }

            return false;
        }

        private Cell EnsureDestinationCell()
        {
            // try to ensure that the destination cell (the first cell for traversal) is not null
            Cell destinationCell = GetCellAtPos(_destination);
            if (destinationCell == null)
            {
                var grid = GetGridFromPosition(_destination);
                if (grid != null)
                {
                    destinationCell = grid.GetNearestWalkableCell(_destination, _destination, false, Mathf.CeilToInt(_boundsPadding + _extraPadding), _unitProperties);
                }
            }

            return destinationCell;
        }

        private void StartFastMarchingMethod()
        {
            Cell destinationCell = EnsureDestinationCell();
            if (destinationCell == null)
            {
                // if there really is no destination cell - i.e. the cell we start traversal at, then just quit
                return;
            }

            _destination = destinationCell.position;
            _openSet.Enqueue(destinationCell);

            while (_openSet.count > 0)
            {
                FastMarchingMethod(_openSet.Dequeue());
            }
        }

        private void FastMarchingMethod(Cell currentCell)
        {
            Vector3 cellPos = currentCell.position;

            // get all walkable neighbours - this method internally checks for whether the cells have been fast marched
            GetWalkableCellNeighbours(currentCell, _tempWalkableNeighbours, !_allowDiagonals);
            int neighboursCount = _tempWalkableNeighbours.count;
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbour = _tempWalkableNeighbours[i];
                Vector3 neighbourPos = neighbour.position;

                // if the cell has unwalkable neighbour cells, then we need to multiply its magnitude with the obstacle strength factor
                bool noUnwalkables = GetCellNeighboursAndUnwalkability(neighbour, _extraTempWalkableNeighbours, !_allowDiagonals);
                float factor = noUnwalkables ? 1f : _obstacleStrengthFactor;

                _fastMarchedCellsSet[neighbourPos] = neighbourPos.DirToXZ(cellPos) * factor;

                if (!_groupBounds.Contains(neighbourPos))
                {
                    // only enqueue cell neighbours that lie within the group bounds area
                    continue;
                }

                _openSet.Enqueue(neighbour);
            }
        }

        private void SmoothFields()
        {
            // iterate over all fast marched cells - this uses an enumerator and thus allocates a small amount of memory
            foreach (var pair in _fastMarchedCellsSet)
            {
                Vector3 pos = pair.Key;
                var cell = GetCellAtPos(pos);

                int smoothVectors = 1;
                PlaneVector smoothVector = pair.Value;

                // get all cell neighbours, if there are no unwalkable (or missing neighbours, if built-in containment is true)
                bool noUnwalkables = GetCellNeighboursAndUnwalkability(cell, _tempWalkableNeighbours, !_allowDiagonals);
                if (noUnwalkables)
                {
                    int walkableNeighboursCount = _tempWalkableNeighbours.count;
                    for (int i = 0; i < walkableNeighboursCount; i++)
                    {
                        var neighbour = _tempWalkableNeighbours[i];
                        Vector3 neighbourPos = neighbour.position;

                        PlaneVector dir = GetFastMarchedCellAtPos(neighbourPos);
                        if (dir.sqrMagnitude == 0f)
                        {
                            // if the neighbour has not been fast marched, then ignore it
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

                // find all the portal neighbour nodes
                int neighbourCount = node.neighbourNodes.Length;
                for (int j = 0; j < neighbourCount; j++)
                {
                    var neighbour = node.neighbourNodes[j];
                    Vector3 neighbourPos = neighbour.position;

                    if (!_groupBounds.Contains(neighbourPos))
                    {
                        // ignore portal neighbour nodes that lie outside of the group bounds area
                        continue;
                    }

                    // all portal neighbour nodes within the group bounds point to the same index in the path, the index where the portal is
                    VectorFieldCell fieldCell;
                    if (_cellDirsSet.TryGetValue(neighbourPos, out fieldCell))
                    {
                        // ignore nodes not in the vector field
                        _cellDirsSet[neighbourPos] = new VectorFieldCell(fieldCell.direction, i);
                    }
                }
            }
        }

        private Cell GetCellNeighbour(Cell cell, int dx, int dz)
        {
            int mx = cell.matrixPosX + dx;
            int mz = cell.matrixPosZ + dz;
            var matrix = cell.parent;

            if (mx >= 0 && mx < matrix.columns &&
                mz >= 0 && mz < matrix.rows)
            {
                // if the requested neighbour is on the same grid, then just get it normally
                return matrix[mx, mz];
            }

            // TODO: all grids need to have same cell size, for this to work - but how else to know which matrix the mx and mz comes from ?
            // if the requested neighbour is not on the same grid, however, we compute the position of the neighbour cell and attempt to get it
            float cellSize = matrix.cellSize;
            Vector3 pos = cell.position + new Vector3(dx * cellSize, 0f, dz * cellSize);
            return GetCellAtPos(pos);
        }

        private bool GetCellNeighboursAndUnwalkability(Cell cell, DynamicArray<Cell> walkableCells, bool preventDiagonals)
        {
            if (walkableCells.count != 0)
            {
                // if the list for holding the cells is not empty, make it empty
                walkableCells.Clear();
            }

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

                    var neighbour = GetCellNeighbour(cell, x, z);
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
                }
            }

            return true;
        }

        private void GetWalkableCellNeighbours(Cell cell, DynamicArray<Cell> walkableCells, bool preventDiagonals)
        {
            if (walkableCells.count != 0)
            {
                // if the list for holding the cells is not empty, make it empty
                walkableCells.Clear();
            }

            // prepare non-diagonal neighbours in all 4 directions (up, down, left, right)
            var n1 = GetCellNeighbour(cell, -1, 0);
            var n2 = GetCellNeighbour(cell, 0, -1);
            var n3 = GetCellNeighbour(cell, 1, 0);
            var n4 = GetCellNeighbour(cell, 0, 1);

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
                        // ... and the cell is walkable, then add it to the list
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
                var n5 = GetCellNeighbour(cell, -1, -1);
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
                var n6 = GetCellNeighbour(cell, -1, 1);
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
                var n7 = GetCellNeighbour(cell, 1, -1);
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
                var n8 = GetCellNeighbour(cell, 1, 1);
                if (n8 != null && !_fastMarchedCellsSet.ContainsKey(n8.position))
                {
                    if (n8.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n8, _unitProperties))
                    {
                        walkableCells.Add(n8);
                    }
                }
            }
        }

        private PlaneVector GetFastMarchedCellAtPos(Vector3 position)
        {
            if (_fastMarchedCellsSet == null)
            {
                // if the fast marched dictionary is not ready yet, then return default value of plane vector (0,0)
                return default(PlaneVector);
            }

            // Returns the fast marched cell value at a specified position
            PlaneVector value;
            _fastMarchedCellsSet.TryGetValue(position, out value);
            return value;
        }

        /// <summary>
        /// Gets the field cell at position.
        /// </summary>
        /// <param name="cell">The grid cell.</param>
        /// <returns>The vector field cell matching the grid cell.</returns>
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

        private Cell GetCellAtPos(Vector3 position)
        {
            // get the grid at the given position
            var grid = GetGridFromPosition(position);
            if (grid != null)
            {
                // return the cell at the given position if it exists
                return grid.GetCell(position);
            }

            return null;
        }

        private void AddGrid(IGrid grid)
        {
            // only add grids that are not null and are not already contained in the grid list
            if (grid == null || _grids.Contains(grid))
            {
                return;
            }

            _grids.Add(grid);
        }

        private IGrid GetGridFromPosition(Vector3 position)
        {
            // find the grid that contains the given position and return it if it exists
            int gridsCount = _grids.Count;
            for (int i = 0; i < gridsCount; i++)
            {
                if (_grids[i].Contains(position))
                {
                    return _grids[i];
                }
            }

            return null;
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

            // set path reference and reset associated counters
            _currentPath = path;
            _currentPathStep = 0;
            _currentLineSegment = 0f;
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
            // call update method with false parameter, because we only want it to recompute the vector field if group has moved enough
            UpdateCrossGridVectorField(false);

            return null;
        }

        /// <summary>
        /// A visualization method called in OnDrawGizmos - only used for debugging, should be empty for production use.
        /// </summary>
        public void DebugRender()
        {
            if (_grids == null || _grids.Count == 0)
            {
                return;
            }

            Vector3 extraHeight = new Vector3(0f, 0.5f, 0f);

            foreach (var pair in _cellDirsSet)
            {
                Vector3 pos = pair.Key + extraHeight;
                var fieldCell = pair.Value;

                if (fieldCell.pathPortalIndex == -1)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }

                Gizmos.DrawSphere(pos, 0.25f);
                Vector3 fieldDirection = fieldCell.direction.normalized;
                Gizmos.DrawLine(pos, pos + fieldDirection);
            }

            if (_destination.sqrMagnitude != 0f)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(_destination, 0.25f);
            }

            if (_groupBounds.center.sqrMagnitude != 0f)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(_groupBounds.center, _groupBounds.size);
            }

            if (_lastGroupCogCell.sqrMagnitude != 0f)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_lastGroupCogCell, 1f);
            }
        }
    }
}