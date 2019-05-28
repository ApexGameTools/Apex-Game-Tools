/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System.Collections.Generic;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.PathFinding.MoveCost;
    using Apex.Services;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Pathing engine using the A* algorithm.
    /// </summary>
    public class PathingAStar : PathingEngineBase
    {
        private BinaryHeap<IPathNode> _openSet;
        private DynamicArray<IPathNode> _successorArray;
        private IPathNode _closestNode;
        private bool _preventDiagonalMoves;

        protected List<IPathNode> _expandedSet;
        protected IPathNode _current;

        /// <summary>
        /// The attributes of the path requester
        /// </summary>
        protected IUnitProperties _unitProps;

        /// <summary>
        /// The unit's attributes
        /// </summary>
        protected AttributeMask _unitAttributes;

        /// <summary>
        /// The whether to allow corner cutting
        /// </summary>
        protected bool _cutCorners;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathingAStar"/> class.
        /// </summary>
        /// <param name="heapInitialSize">Initial size of the heap.</param>
        public PathingAStar(int heapInitialSize)
            : base(new DiagonalDistance(10), new DefaultCellCostStrategy(), new PathSmoother(), null)
        {
            _openSet = new BinaryHeap<IPathNode>(heapInitialSize, new PathNodeComparer());
            _expandedSet = new List<IPathNode>();
            _successorArray = new DynamicArray<IPathNode>(15);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathingAStar"/> class.
        /// </summary>
        /// <param name="heapInitialSize">Initial size of the heap.</param>
        /// <param name="moveCostProvider">The move cost provider.</param>
        /// <param name="cellCostStrategy">The cell cost provider.</param>
        /// <param name="pathSmoother">The path smoother to use.</param>
        /// <param name="requestPreprocessors">The list of request preprocessors to use.</param>
        public PathingAStar(int heapInitialSize, IMoveCost moveCostProvider, ICellCostStrategy cellCostStrategy, ISmoothPaths pathSmoother, IRequestPreProcessor[] requestPreprocessors)
            : base(moveCostProvider, cellCostStrategy, pathSmoother, requestPreprocessors)
        {
            _openSet = new BinaryHeap<IPathNode>(heapInitialSize, new PathNodeComparer());
            _expandedSet = new List<IPathNode>();
            _successorArray = new DynamicArray<IPathNode>(15);
        }

        /// <summary>
        /// Gets the walkable successors of the specified node.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="successorArray">The array to fill with successors.</param>
        /// <returns>All walkable successors of the node.</returns>
        protected virtual void GetWalkableSuccessors(IPathNode current, DynamicArray<IPathNode> successorArray)
        {
            current.GetWalkableNeighbours(successorArray, _unitProps, _cutCorners, _preventDiagonalMoves);
        }

        /// <summary>
        /// Called when a request is about to be processed.
        /// </summary>
        /// <param name="start">The start node.</param>
        /// <param name="actualStart">The actual start node in case actual start is blocked.</param>
        protected override void OnStart(IPathNode start, IPathNode actualStart)
        {
            _openSet.Clear();

            //Reset all g's on all nodes marking them not expanded
            _expandedSet.Apply(c => c.g = 0);
            _expandedSet.Clear();
            _closestNode = start;

            //Initialize and add the start node. Since no expanded set is used start g must be initialized to a small > 0  value to mark it as expanded
            //Since the start node will never reenter the open set, there is no need to initialize the h and f values.
            start.g = 1;
            start.h = this.costProvider.GetHeuristic(start, this.goal);
            start.predecessor = actualStart;

            _openSet.Add(start);
            _expandedSet.Add(start);

            if (actualStart != null)
            {
                actualStart.g = 1;
                actualStart.predecessor = null;
                _expandedSet.Add(actualStart);
            }

            _unitProps = this.currentRequest.requesterProperties;
            _unitAttributes = _unitProps.attributes;
            _preventDiagonalMoves = this.currentRequest.pathFinderOptions.preventDiagonalMoves;
            _cutCorners = this.currentRequest.pathFinderOptions.allowCornerCutting;
        }

        /// <summary>
        /// Processes the next node.
        /// </summary>
        /// <returns>The current pathing status</returns>
        protected override PathingStatus ProcessNext()
        {
            //First we need to explore the various exit conditions
            if (!_openSet.hasNext)
            {
                if (this.currentRequest.pathFinderOptions.navigateToNearestIfBlocked)
                {
                    UpdateGoal(_closestNode);
                    return PathingStatus.Complete;
                }

                if (!this.goal.IsWalkableWithClearance(_unitProps))
                {
                    //Goal became unwalkable during a star, which can happen in both synchronous and async operation, since synchronous can have part of its processing split across frames if the time threshold is hit.
                    return PathingStatus.DestinationBlocked;
                }

                //All routes have been explored without finding a route to destination, i.e. one does not (currently) exist
                return PathingStatus.NoRouteExists;
            }

            //I doubt there is any reason to have this added complexity that the user needs to consider, just resize
            if (_openSet.count == _openSet.capacity)
            {
                UnityServices.debug.LogWarning("The pathing engine's heap capacity (" + _openSet.capacity + ") has been reached. If this happens often, it is recommended that you increase the initial heap size of the engine to avoid automatic resizing.\nDo this on the Path Service Component and increment it by 10 or so at a time to keep it as small as possible.");
                _openSet.Resize();
            }

            _current = _openSet.Remove();
            if (_current == this.goal)
            {
                //Hurray we found a route
                return PathingStatus.Complete;
            }

            if (_closestNode.h > _current.h)
            {
                _closestNode = _current;
            }

            _current.isClosed = true;

            //Get potential successors
            _successorArray.Clear();
            GetWalkableSuccessors(_current, _successorArray);
            _current.GetVirtualNeighbours(_successorArray, _unitProps);

            //Assign costs
            var walkableCount = _successorArray.count;
            for (int i = 0; i < walkableCount; i++)
            {
                var n = _successorArray[i];
                if (n == null)
                {
                    break;
                }

                var cost = _current.g + this.cellCostStrategy.GetCellCost(n, _unitProps);

                var portal = _current as IPortalNode;
                if (portal != null)
                {
                    cost += portal.GetCost(portal.predecessor, n, this.costProvider);
                }
                else if (!(n is IPortalNode))
                {
                    cost += this.costProvider.GetMoveCost(_current, n);
                }

                //Instead of operating with an expanded set (closed set + open set) we simply evaluate the g value of the node, since g will only ever be > 0 if the node has been expanded.
                //This saves lots of memory over time and is even more O(1) than checking a hash set
                if (n.g > 0)
                {
                    //In the case that the neighbour has been evaluated in context to another node but gets a better cost in relation the current node, update its state.
                    if (n.g > cost)
                    {
                        n.g = cost;
                        n.f = cost + n.h;
                        n.predecessor = _current;

                        //If the node is closed, i.e. was removed from the open set, open it again. Otherwise update its position in the open set.
                        if (n.isClosed)
                        {
                            n.isClosed = false;
                            _openSet.Add(n);
                        }
                        else
                        {
                            _openSet.ReheapifyUpFrom(n);
                        }
                    }
                }
                else
                {
                    //Add the node to the open set
                    n.g = cost;
                    n.h = GetHeuristic(n, _current);
                    n.f = cost + n.h;
                    n.predecessor = _current;
                    n.isClosed = false;

                    _openSet.Add(n);
                    _expandedSet.Add(n);
                }
            }

            return PathingStatus.Running;
        }

        private int GetHeuristic(IPathNode n, IPathNode current)
        {
            var portal = n as IPortalNode;
            if (portal != null)
            {
                //Since portals are co-positioned with the nodes from which they are accessed, they simply have the same h as their predecessor
                //But since portals.predecessor.h is error prone (predecessor must have been set before calling this) we pass in current.
                return current.h;
            }

            var best = this.costProvider.GetHeuristic(n, this.goal);

            //iterate over relevant portals to see if that gets a better score
            var portals = n.parent.shortcutPortals;
            for (int i = 0; i < portals.Count; i++)
            {
                var p = portals[i];
                if (p.IsWalkableWithClearance(_unitProps))
                {
                    var h = p.GetHeuristic(n, this.goal, this.costProvider);
                    if (h < best)
                    {
                        best = h;
                    }
                }
            }

            return best;
        }
    }
}
