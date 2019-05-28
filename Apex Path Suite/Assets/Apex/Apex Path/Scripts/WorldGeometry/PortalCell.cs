/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Linq;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.PathFinding.MoveCost;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Represents a virtual cell that represents one end of a portal.
    /// </summary>
    public class PortalCell : IPortalNode
    {
        private GridPortal _parentPortal;
        private IPortalAction _action;
        private PortalCell _partner;
        private IPathNode[] _neighbourNodes;
        private MatrixBounds _matrixBounds;
        private bool _isOneWay;

        internal PortalCell(GridPortal parentPortal, bool oneWay, IPortalAction action)
        {
            _parentPortal = parentPortal;
            _isOneWay = oneWay;
            _action = action;
        }

        internal IPathNode[] neighbourNodes
        {
            get { return _neighbourNodes; }
        }

        /// <summary>
        /// Gets the portal to which this cell belongs.
        /// </summary>
        /// <value>
        /// The portal.
        /// </value>
        public GridPortal portal
        {
            get { return _parentPortal; }
        }

        /// <summary>
        /// Gets the partner portal cell.
        /// </summary>
        /// <value>
        /// The partner.
        /// </value>
        public IPortalNode partner
        {
            get { return _partner; }
        }

        /// <summary>
        /// Gets or sets the arbitrary cost of walking this cell.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public int cost
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the position of the component.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parent cell matrix.
        /// </summary>
        /// <value>
        /// The parent matrix.
        /// </value>
        public CellMatrix parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this cell is permanently blocked.
        /// Note that this is automatically set depending on level geometry when the Grid initializes, but it can also be changed manually.
        /// </summary>
        public bool isPermanentlyBlocked
        {
            get { return false; }
            set { /* NOOP */ }
        }

        int IPathNode.g
        {
            get;
            set;
        }

        int IPathNode.h
        {
            get;
            set;
        }

        int IPathNode.f
        {
            get;
            set;
        }

        IPathNode IPathNode.predecessor
        {
            get;
            set;
        }

        bool IPathNode.isClosed
        {
            get;
            set;
        }

        bool IPathNode.hasVirtualNeighbour
        {
            get { return false; }
        }

        int IGridCell.matrixPosX
        {
            get { return -1; }
        }

        int IGridCell.matrixPosZ
        {
            get { return -1; }
        }

        NeighbourPosition IGridCell.neighbours
        {
            get { return NeighbourPosition.None; }
        }

        /// <summary>
        /// Gets the heuristic for a node in relation to this portal. This is only used for Shortcut portals, but is valid for all types.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="goal">The goal.</param>
        /// <param name="moveCostProvider">The move cost provider.</param>
        /// <returns>The heuristic</returns>
        public int GetHeuristic(IPathNode node, IPathNode goal, IMoveCost moveCostProvider)
        {
            //The logic here is that we want to shortest possible distance from the node to this portal cell,
            //combined with the shortest possible distance from our partner portal cell to the goal, again combined with the
            // cost of making the portal move using those two entry points.
            int mx = _matrixBounds.AdjustColumnToBounds(node.matrixPosX);
            int mz = _matrixBounds.AdjustRowToBounds(node.matrixPosZ);
            var closestCellToNode = this.parent.rawMatrix[mx, mz];

            mx = _partner._matrixBounds.AdjustColumnToBounds(goal.matrixPosX);
            mz = _partner._matrixBounds.AdjustRowToBounds(goal.matrixPosZ);
            var closestCellToGoal = _partner.parent.rawMatrix[mx, mz];

            return moveCostProvider.GetHeuristic(node, closestCellToNode) + _action.GetActionCost(closestCellToNode, closestCellToGoal, moveCostProvider) + moveCostProvider.GetHeuristic(closestCellToGoal, goal);
        }

        /// <summary>
        /// Gets the action cost.
        /// </summary>
        /// <param name="from">The node from which the action will start.</param>
        /// <param name="to">The node at which the action will end.</param>
        /// <param name="costProvider">The cost provider in use by the path finder.</param>
        /// <returns>The cost</returns>
        public int GetCost(IPositioned from, IPositioned to, IMoveCost costProvider)
        {
            return _action.GetActionCost(from, to, costProvider);
        }

        /// <summary>
        /// Executes the portal move.
        /// </summary>
        /// <param name="unit">The unit that is entering the portal.</param>
        /// <param name="to">The destination at the other side of the portal.</param>
        /// <param name="callWhenComplete">The callback to call when the move is complete.</param>
        /// <returns>The grid of the destination.</returns>
        public IGrid Execute(Transform unit, IPositioned to, Action callWhenComplete)
        {
            _action.Execute(unit, this, to, callWhenComplete);

            return _parentPortal.GetGridFor(this);
        }

        /// <summary>
        /// Determines whether the cell is walkable.
        /// </summary>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns>
        ///   <c>true</c> if the cell is walkable, otherwise <c>false</c>
        /// </returns>
        public bool IsWalkableWithClearance(IUnitProperties unitProps)
        {
            return _parentPortal.IsUsableBy(unitProps.attributes);
        }

        /// <summary>
        /// Determines whether the cell is walkable. This check does not take unit size into consideration.
        /// </summary>
        /// <param name="mask">The unit's attributes.</param>
        /// <returns>
        ///   <c>true</c> if the cell is walkable, otherwise <c>false</c>
        /// </returns>
        public bool IsWalkable(AttributeMask mask)
        {
            return _parentPortal.IsUsableBy(mask);
        }

        /// <summary>
        /// Checks if the specified cell is se4rvices by this portal, i.e. if this portal is usable from the cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns><see langword="true"/> if the portal is usable from the cell; otherwise <see langword="false"/></returns>
        public bool Services(IGridCell cell)
        {
            return _matrixBounds.Contains(cell.matrixPosX, cell.matrixPosZ);
        }

        bool IGridCell.IsWalkableFromAllDirections(IUnitProperties unitProps)
        {
            return IsWalkableWithClearance(unitProps);
        }

        bool IGridCell.IsWalkableFrom(IGridCell neighbour, IUnitProperties unitProps)
        {
            return _parentPortal.IsUsableBy(unitProps.attributes);
        }

        bool IGridCell.IsWalkableFromWithClearance(IGridCell neighbour, IUnitProperties unitProps)
        {
            return _parentPortal.IsUsableBy(unitProps.attributes);
        }

        NeighbourPosition IGridCell.GetRelativePositionTo(IGridCell other)
        {
            return NeighbourPosition.None;
        }

        VectorXZ IGridCell.GetDirectionTo(IGridCell other)
        {
            return MatrixDirection.None;
        }

        Cell IGridCell.GetNeighbour(VectorXZ offset)
        {
            return null;
        }

        Cell IGridCell.GetNeighbour(int dx, int dz)
        {
            return null;
        }

        int IGridCell.GetNeighbours(Cell[] neighboursBuffer)
        {
            return 0;
        }

        void IPathNode.GetWalkableNeighbours(DynamicArray<IPathNode> neighbours, IUnitProperties unitProps, bool cornerCuttingAllowed, bool preventDiagonalMoves)
        {
            var destinationNodes = _partner._neighbourNodes;
            var nodeCount = destinationNodes.Length;

            if (_parentPortal.type == PortalType.Connector)
            {
                //This logic depends on the prerequisite that connector portals always are only one cell wide or high, such that the cells covered by the portal represents a simple array.
                //The prerequisite also demands that connector portals are symmetric and placed directly across from one another, they cannot be offset, e.g. cell one on one side must link to cell one on the other side.
                //We then simply find the index of the cell being evaluated (the portal predecessor) and only return maximum 3 cell on the other side, its immediate partner plus each diagonal if they exist.
                //  _______________
                // |___|_N_|_N_|_N_|
                // |___|___|_P_|___|
                //
                //This accomplished two things, First of all we reduce the number of cells being evaluated. Second the resulting path will cross the portal as quickly as possible and not make long diagonal moves across a portal.
                var p = ((IPathNode)this).predecessor;
                var idx = _matrixBounds.IndexOf(p.matrixPosX, p.matrixPosZ);
                if (idx < 0)
                {
                    return;
                }

                if (preventDiagonalMoves)
                {
                    if (destinationNodes[idx].IsWalkableWithClearance(unitProps))
                    {
                        neighbours.Add(destinationNodes[idx]);
                    }
                }
                else
                {
                    var start = Math.Max(idx - 1, 0);
                    var end = Math.Min(idx + 2, nodeCount);

                    for (int i = start; i < end; i++)
                    {
                        if (destinationNodes[i].IsWalkableWithClearance(unitProps))
                        {
                            neighbours.Add(destinationNodes[i]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < nodeCount; i++)
                {
                    if (destinationNodes[i].IsWalkableWithClearance(unitProps))
                    {
                        neighbours.Add(destinationNodes[i]);
                    }
                }
            }
        }

        bool IPathNode.TryGetWalkableNeighbour(int dx, int dz, IUnitProperties unitProps, DynamicArray<IPathNode> neighbours)
        {
            return false;
        }

        void IPathNode.GetVirtualNeighbours(DynamicArray<IPathNode> neighbours, IUnitProperties unitProps)
        {
            /* Currently not supported */
        }

        void IPathNode.RegisterVirtualNeighbour(IPathNode neighbour)
        {
            /* Currently not supported */
        }

        void IPathNode.UnregisterVirtualNeighbour(IPathNode neighbour)
        {
            /* Currently not supported */
        }

        internal IGrid Initialize(PortalCell partner, Bounds portalBounds)
        {
            var grid = GridManager.instance.GetGrid(portalBounds.center);
            if (grid == null)
            {
                return null;
            }

            _partner = partner;
            this.parent = grid.cellMatrix;
            this.position = portalBounds.center;

            _matrixBounds = grid.cellMatrix.GetMatrixBounds(portalBounds, 0.0f, true);
            _neighbourNodes = grid.cellMatrix.GetRange(_matrixBounds).ToArray();

            return grid;
        }

        internal void Activate()
        {
            if (_isOneWay)
            {
                return;
            }

            if (_parentPortal.type == PortalType.Shortcut)
            {
                this.parent.shortcutPortals.Add(this);
            }

            for (int i = 0; i < _neighbourNodes.Length; i++)
            {
                _neighbourNodes[i].RegisterVirtualNeighbour(this);
            }
        }

        internal void Deactivate()
        {
            if (_parentPortal.type == PortalType.Shortcut)
            {
                this.parent.shortcutPortals.Remove(this);
            }

            for (int i = 0; i < _neighbourNodes.Length; i++)
            {
                _neighbourNodes[i].UnregisterVirtualNeighbour(this);
            }
        }
    }
}
