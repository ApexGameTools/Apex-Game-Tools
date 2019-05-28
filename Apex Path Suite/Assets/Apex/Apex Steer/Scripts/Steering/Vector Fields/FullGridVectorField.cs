/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.VectorFields
{
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.PathFinding;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A vector field which spans an entire grid, but its vector field is only updated on replanning or similar.
    /// Only allocates memory and hits performance once (per replanning/move order).
    /// Supports portals that actually move units in their action (e.g. jump, teleport), i.e. does not support grid stitching portals (connectors).
    /// Supports only 1 grid. If multiple grids are used, the solution is to use CrossGridVectorField.
    /// </summary>
    public class FullGridVectorField : IVectorField, ILoadBalanced
    {
        private float _obstacleStrengthFactor;
        private bool _allowCornerCutting;
        private bool _allowDiagonals;
        private bool _announceAllNodes;
        private bool _builtInContainment;
        private float _updateInterval;

        private bool _onFinalApproach;

        private IUnitProperties _unitProperties;
        private Path _currentPath;
        private Vector3 _destination;
        private IGrid _grid;

        private PlaneVector[,] _fastMarchedCells;
        private VectorFieldCell[,] _cellDirs;

        private DynamicArray<Cell> _tempWalkableNeighbours;
        private DynamicArray<Cell> _extraTempWalkableNeighbours;
        private SimpleQueue<Cell> _openSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="FullGridVectorField"/> class.
        /// </summary>
        /// <param name="group">The transient unit group.</param>
        /// <param name="path">The path.</param>
        /// <param name="options">The vector field options.</param>
        public FullGridVectorField(TransientGroup<IUnitFacade> group, Path path, VectorFieldOptions options)
        {
            Ensure.ArgumentNotNull(group, "group");
            this.group = group;

            _currentPath = path;

            var modelUnit = group.modelUnit;
            _unitProperties = modelUnit;
            var pathOptions = modelUnit.pathFinderOptions;

            // cache options locally
            _obstacleStrengthFactor = options.obstacleStrengthFactor;
            _allowCornerCutting = pathOptions.allowCornerCutting;
            _allowDiagonals = !pathOptions.preventDiagonalMoves;
            _announceAllNodes = modelUnit.pathNavigationOptions.announceAllNodes;

            _builtInContainment = options.builtInContainment;
            _updateInterval = options.updateInterval;

            // pre-allocate lists
            _openSet = new SimpleQueue<Cell>(31);
            _tempWalkableNeighbours = new DynamicArray<Cell>(8);
            _extraTempWalkableNeighbours = new DynamicArray<Cell>(8);

            _grid = GridManager.instance.GetGrid(group.modelUnit.position);
            if (_grid != null)
            {
                _fastMarchedCells = new PlaneVector[_grid.sizeX, _grid.sizeZ];
                _cellDirs = new VectorFieldCell[_grid.sizeX, _grid.sizeZ];
            }
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
        /// Initializes this vector field (called right after being instantiated).
        /// </summary>
        public void Initialize()
        {
            GenerateVectorField();

            NavLoadBalancer.steering.Add(this, _updateInterval, true);
        }

        private void ResetField()
        {
            var zero = PlaneVector.zero;

            // reset the value of all field cells
            int sizeX = _grid.sizeX, sizeZ = _grid.sizeZ;
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    _fastMarchedCells[x, z] = zero;
                }
            }
        }

        private void GenerateVectorField()
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

            // the order of these methods is important, must be called in this order
            StartFastMarchingMethod();
            SmoothFields();
            HandlePortals();
        }

        private void StartFastMarchingMethod()
        {
            _destination = _currentPath.Last().position;

            // find the first cell used for traversal - i.e. the path destination
            // note however, that the destination must be the first found portal node in the path if it exists - we must never go past this
            int pathCount = _currentPath.count - 1;
            for (int i = 1; i < pathCount; i++)
            {
                var pathNode = _currentPath[i];
                if (pathNode is IPortalNode)
                {
                    _destination = _currentPath[i - 1].position;
                    break;
                }
            }

            Cell destinationCell = _grid.GetCell(_destination, true);
            _openSet.Enqueue(destinationCell);

            while (_openSet.count > 0)
            {
                FastMarchingMethod(_openSet.Dequeue());
            }
        }

        private void FastMarchingMethod(Cell currentCell)
        {
            var cellPos = currentCell.position;

            // get all walkable cell neighbours
            GetWalkableCellNeighbours(currentCell, _tempWalkableNeighbours, !_allowDiagonals);
            int neighboursCount = _tempWalkableNeighbours.count;
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbour = _tempWalkableNeighbours[i];
                int mx = neighbour.matrixPosX;
                int mz = neighbour.matrixPosZ;

                // if the cell has already been fast marched, then ignore it
                if (_fastMarchedCells[mx, mz].sqrMagnitude != 0f)
                {
                    continue;
                }

                // if the cell has unwalkable neighbour cells, then we need to multiply its magnitude with the obstacle strength factor
                bool noUnwalkables = GetCellNeighboursAndUnwalkability(neighbour, _extraTempWalkableNeighbours, !_allowDiagonals);
                float factor = noUnwalkables ? 1f : _obstacleStrengthFactor;

                // make the fast marched cell point towards its traversal-predecessor
                Vector3 neighbourPos = neighbour.position;
                _fastMarchedCells[mx, mz] = neighbourPos.DirToXZ(cellPos) * factor;

                _openSet.Enqueue(neighbour);
            }
        }

        private void SmoothFields()
        {
            var zero = PlaneVector.zero;

            // loop over entire grid
            int sizeX = _grid.sizeX, sizeZ = _grid.sizeZ;
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    PlaneVector direction = _fastMarchedCells[x, z];
                    if (direction.sqrMagnitude == 0f)
                    {
                        _cellDirs[x, z].direction = zero;
                        _cellDirs[x, z].pathPortalIndex = -1;
                        // if the given cell has not been fast marched, it should not be smoothed, and reset it to default
                        continue;
                    }

                    // smooth the cell's vector, if and only if it has no blocked cell neighbours (or missing, if built-in containment is true)
                    int smoothVectors = 1;
                    PlaneVector smoothVector = direction;

                    var cell = _grid.cellMatrix[x, z];
                    bool noUnwalkables = GetCellNeighboursAndUnwalkability(cell, _tempWalkableNeighbours, !_allowDiagonals);
                    if (noUnwalkables)
                    {
                        int walkableNeighboursCount = _tempWalkableNeighbours.count;
                        for (int i = 0; i < walkableNeighboursCount; i++)
                        {
                            var neighbour = _tempWalkableNeighbours[i];

                            PlaneVector dir = _fastMarchedCells[neighbour.matrixPosX, neighbour.matrixPosZ];
                            if (dir.sqrMagnitude == 0f)
                            {
                                // if the neighbour has not been fast marched, then ignore it
                                continue;
                            }

                            smoothVectors++;
                            smoothVector += dir;
                        }

                        // divide by the amount of vectors summed to get back to the original magnitude
                        smoothVector /= smoothVectors;
                    }

                    // update the vector field cell
                    _cellDirs[x, z].direction = smoothVector;
                    _cellDirs[x, z].pathPortalIndex = -1;
                } /* end grid traversal z */
            } /* end grid traversal x */
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
                    // if the path node is not on this vector field's grid, then ignore it
                    continue;
                }

                // find all the portal neighbour nodes
                int neighbourCount = node.neighbourNodes.Length;
                for (int j = 0; j < neighbourCount; j++)
                {
                    var neighbour = node.neighbourNodes[j];

                    int mx = neighbour.matrixPosX;
                    int mz = neighbour.matrixPosZ;

                    // all portal neighbour nodes point to the same index in the path, the index where the portal is
                    _cellDirs[mx, mz].pathPortalIndex = i;
                }
            }
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
                lw = _fastMarchedCells[mx - 1, mz].sqrMagnitude != 0f;
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
                dw = _fastMarchedCells[mx, mz - 1].sqrMagnitude != 0f;
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
                rw = _fastMarchedCells[mx + 1, mz].sqrMagnitude != 0f;
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
                uw = _fastMarchedCells[mx, mz + 1].sqrMagnitude != 0f;
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
                if (n5 != null && _fastMarchedCells[mx - 1, mz - 1].sqrMagnitude == 0f)
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
                if (n6 != null && _fastMarchedCells[mx - 1, mz + 1].sqrMagnitude == 0f)
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
                if (n7 != null && _fastMarchedCells[mx + 1, mz - 1].sqrMagnitude == 0f)
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
                if (n8 != null && _fastMarchedCells[mx + 1, mz + 1].sqrMagnitude == 0f)
                {
                    if (n8.IsWalkableWithClearance(_unitProperties) && cell.IsWalkableFromWithClearance(n8, _unitProperties))
                    {
                        walkableCells.Add(n8);
                    }
                }
            }
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
            return _cellDirs[cell.matrixPosX, cell.matrixPosZ];
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
                _onFinalApproach = false;

                GenerateVectorField();
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

        private void PopPathNode()
        {
            // need to re-run HandlePortals after popping a node, to make sure the path portal index is correct
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
        /// A visualization method called in OnDrawGizmos - only used for debugging, should be empty for production use.
        /// </summary>
        public void DebugRender()
        {
            if (_grid == null)
            {
                return;
            }

            Vector3 extraHeight = new Vector3(0f, 0.5f, 0f);

            int sizeX = _grid.sizeX, sizeZ = _grid.sizeZ;
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Vector3 pos = _grid.cellMatrix[x, z].position + extraHeight;
                    var fieldCell = _cellDirs[x, z];

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

            if (_destination.sqrMagnitude != 0f)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(_destination, 0.25f);
            }
        }
    }
}