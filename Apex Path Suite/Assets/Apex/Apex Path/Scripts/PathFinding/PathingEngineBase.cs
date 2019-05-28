/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.PathFinding.MoveCost;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Base class for pathing engines
    /// </summary>
    public abstract class PathingEngineBase : IPathingEngine
    {
        private readonly ICellCostStrategy _cellCostStrategy;
        private readonly IMoveCost _costProvider;
        private readonly ISmoothPaths _smoother;
        private IPathNode _goal;
        private PathResult _currentResult;
        private SafeIterator _coroutineIter;
        private SegmentRequest _segmentRequest;
        private DynamicArray<Path> _segments;
        private IRequestPreProcessor[] _requestPreprocessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathingEngineBase"/> class.
        /// </summary>
        /// <param name="moveCostProvider">The move cost provider.</param>
        /// <param name="cellCostStrategy">The cell cost provider.</param>
        /// <param name="smoother">The path smoother to use</param>
        /// <param name="requestPreprocessors">The list of request preprocessors to use.</param>
        protected PathingEngineBase(IMoveCost moveCostProvider, ICellCostStrategy cellCostStrategy, ISmoothPaths smoother, IRequestPreProcessor[] requestPreprocessors)
        {
            Ensure.ArgumentNotNull(moveCostProvider, "moveCostProvider");
            Ensure.ArgumentNotNull(cellCostStrategy, "cellCostStrategy");
            Ensure.ArgumentNotNull(smoother, "smoother");

            _costProvider = moveCostProvider;
            _smoother = smoother;
            _cellCostStrategy = cellCostStrategy;
            _coroutineIter = new SafeIterator(this);
            _segmentRequest = new SegmentRequest();
            _segments = new DynamicArray<Path>(10);
            _requestPreprocessors = requestPreprocessors;
        }

        /// <summary>
        /// Gets the cost provider.
        /// </summary>
        /// <value>
        /// The cost provider.
        /// </value>
        protected IMoveCost costProvider
        {
            get { return _costProvider; }
        }

        /// <summary>
        /// Gets the cell cost strategy.
        /// </summary>
        /// <value>
        /// The cell cost strategy.
        /// </value>
        protected ICellCostStrategy cellCostStrategy
        {
            get { return _cellCostStrategy; }
        }

        /// <summary>
        /// Gets the path smoother.
        /// </summary>
        /// <value>
        /// The path smoother.
        /// </value>
        protected ISmoothPaths pathSmoother
        {
            get { return _smoother; }
        }

        /// <summary>
        /// Gets the current request.
        /// </summary>
        /// <value>
        /// The current request.
        /// </value>
        protected IPathRequest currentRequest
        {
            get { return _segmentRequest; }
        }

        /// <summary>
        /// Gets the goal, i.e. final destination.
        /// </summary>
        /// <value>
        /// The goal.
        /// </value>
        protected IPathNode goal
        {
            get { return _goal; }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void ProcessRequest(IPathRequest request)
        {
            try
            {
                //Granted this could be done smarter, but since this is not an expected scenario, we just use exception handling to handle it.
                ValidateRequest(request);

                _currentResult = new PathResult(PathingStatus.Complete, null, 0, request);

                var status = PathingStatus.Complete;

                //Iterate over the waypoints of the request, including the end point
                _segmentRequest.Start(request);

                do
                {
                    status = StartPathSegment();
                    if (status != PathingStatus.Running)
                    {
                        break;
                    }

                    while (status == PathingStatus.Running)
                    {
                        status = ProcessNext();
                    }

                    if (!CompletePathSegment(status))
                    {
                        break;
                    }
                }
                while (_segmentRequest.MoveNext());

                if (status != PathingStatus.Complete)
                {
                    if (_segments.count > 0)
                    {
                        _currentResult.RegisterPartialResult(status, _segmentRequest.GetPendingWaypoints());
                    }
                    else
                    {
                        _currentResult.status = status;
                    }
                }

                //Splice the path segments and complete the request
                _currentResult.path = Path.FromSegments(_segments);
                request.Complete(_currentResult);
            }
            catch (Exception e)
            {
                FailRequest(request, e);
            }
            finally
            {
                _segments.Clear();
                _segmentRequest.Clear();
                _currentResult = null;
            }
        }

        /// <summary>
        /// Processes the request as a coroutine.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The coroutine enumerator</returns>
        public IEnumerator ProcessRequestCoroutine(IPathRequest request)
        {
            _coroutineIter.Init(request);
            return _coroutineIter;
        }

        /// <summary>
        /// Updates the goal to a new value.
        /// </summary>
        /// <param name="newGoal">The new goal.</param>
        protected void UpdateGoal(IPathNode newGoal)
        {
            _goal = newGoal;
            _segmentRequest.to = newGoal.position;
        }

        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <exception cref="System.ArgumentException">The request is invalid.;request</exception>
        /// <exception cref="System.InvalidOperationException">A new request cannot be started while another request is being processed.</exception>
        protected virtual void ValidateRequest(IPathRequest request)
        {
            Ensure.ArgumentNotNull(request, "request");

            if (!request.isValid)
            {
                throw new ArgumentException("The request is invalid.", "request");
            }

            if (_segmentRequest.isRunning)
            {
                throw new InvalidOperationException("A new request cannot be started while another request is being processed.");
            }
        }

        private IEnumerator ProcessRequestCoroutineInternal(IPathRequest request)
        {
            //Granted this could be done smarter, but since this is not an expected scenario, we just use exception handling to handle it.
            ValidateRequest(request);

            _currentResult = new PathResult(PathingStatus.Complete, null, 0, request);

            var status = PathingStatus.Complete;

            //Iterate over the waypoints of the request, including the end point
            _segmentRequest.Start(request);

            do
            {
                status = StartPathSegment();
                if (status != PathingStatus.Running)
                {
                    break;
                }

                while (status == PathingStatus.Running)
                {
                    status = ProcessNext();
                    yield return null;
                }

                if (!CompletePathSegment(status))
                {
                    break;
                }
            }
            while (_segmentRequest.MoveNext());

            if (status != PathingStatus.Complete)
            {
                if (_segments.count > 0)
                {
                    _currentResult.RegisterPartialResult(status, _segmentRequest.GetPendingWaypoints());
                }
                else
                {
                    _currentResult.status = status;
                }
            }

            //Splice the path segments and complete the request
            _currentResult.path = Path.FromSegments(_segments);
            request.Complete(_currentResult);

            _segments.Clear();
            _segmentRequest.Clear();
            _currentResult = null;
        }

        private PathingStatus StartPathSegment()
        {
            if (_requestPreprocessors != null)
            {
                for (int i = 0; i < _requestPreprocessors.Length; i++)
                {
                    if (_requestPreprocessors[i].PreProcess(_segmentRequest))
                    {
                        break;
                    }
                }
            }

            var fromGrid = GridManager.instance.GetGrid(_segmentRequest.from);
            var toGrid = GridManager.instance.GetGrid(_segmentRequest.to);

            var fromPos = new Position(_segmentRequest.from);
            var toPos = new Position(_segmentRequest.to);

            //If no grids were resolved for this request it means the request involves two points outside the grid(s) that do not cross any grid(s), so we can move directly between them
            if (fromGrid == null && toGrid == null)
            {
                var pathSegment = new Path(2);
                pathSegment.Push(toPos);
                pathSegment.Push(fromPos);
                _segments.Add(pathSegment);
                _currentResult.pathCost = _costProvider.GetHeuristic(fromPos, toPos);
                return PathingStatus.Complete;
            }
            else if (fromGrid == null)
            {
                return PathingStatus.StartOutsideGrid;
            }

            var options = _segmentRequest.pathFinderOptions;
            if (options.optimizeUnobstructedPaths && options.usePathSmoothing && fromGrid == toGrid && !options.preventDiagonalMoves && PathSmoother.CanReducePath(fromPos, toPos, _segmentRequest.requesterProperties, fromGrid.cellMatrix, _cellCostStrategy))
            {
                var pathSegment = new Path(2);
                pathSegment.Push(toPos);
                pathSegment.Push(fromPos);
                _segments.Add(pathSegment);
                _currentResult.pathCost = _costProvider.GetHeuristic(fromPos, toPos);
                return PathingStatus.Complete;
            }

            //Just treat the to grid as the from grid in this case so the destination can be closest point on from grid
            if (toGrid == null)
            {
                toGrid = fromGrid;
            }

            //Get a reference to the start and end cells
            var start = fromGrid.GetCell(_segmentRequest.from, false) as IPathNode;
            _goal = toGrid.GetCell(_segmentRequest.to, false) as IPathNode;

            //Ensure that the end cell is valid
            if (_goal == null)
            {
                if (options.navigateToNearestIfBlocked)
                {
                    _goal = toGrid.GetCell(_segmentRequest.to, true) as IPathNode;
                    _segmentRequest.to = _goal.position;
                }
                else
                {
                    return PathingStatus.EndOutsideGrid;
                }
            }

            if (!_goal.IsWalkableWithClearance(_segmentRequest.requesterProperties) && !options.navigateToNearestIfBlocked)
            {
                return PathingStatus.DestinationBlocked;
            }

            //Ensure that the start cell is valid
            IPathNode actualStart = null;
            if (!start.IsWalkableWithClearance(_segmentRequest.requesterProperties))
            {
                actualStart = start;
                start = fromGrid.GetNearestWalkableCell(start.position, _goal.position, true, options.maxEscapeCellDistanceIfOriginBlocked, _segmentRequest.requesterProperties);
                if (start == null)
                {
                    return PathingStatus.NoRouteExists;
                }
            }

            OnStart(start, actualStart);

            return PathingStatus.Running;
        }

        private bool CompletePathSegment(PathingStatus status)
        {
            if (status != PathingStatus.Complete)
            {
                return false;
            }

            _currentResult.pathCost += _goal.g;

            //Fix the actual destination so it does not overlap obstructions
            FixupGoal(_segmentRequest);

            if (_segmentRequest.type == RequestType.IntelOnly)
            {
                return true;
            }

            var maxPathLength = Mathf.CeilToInt(_goal.g / (_goal.parent.cellSize * _costProvider.baseMoveCost));

            Path pathSegment;
            if (_segmentRequest.pathFinderOptions.usePathSmoothing)
            {
                pathSegment = _smoother.Smooth(_goal, maxPathLength, _segmentRequest, _cellCostStrategy);
            }
            else
            {
                pathSegment = new Path(maxPathLength);

                //Push the actual end position as the goal
                pathSegment.Push(new Position(_segmentRequest.to));

                IPathNode current = _goal.predecessor;
                while (current != null)
                {
                    pathSegment.Push(current);
                    current = current.predecessor;
                }

                //Instead of testing for it in the while loop, just pop off the start node and replace it with the actual start position
                if (pathSegment.count > 1)
                {
                    pathSegment.Pop();
                }

                pathSegment.Push(new Position(_segmentRequest.from));
            }

            _segments.Add(pathSegment);

            return true;
        }

        private void FailRequest(IPathRequest request, Exception e)
        {
            _segments.Clear();
            _segmentRequest.Clear();
            _currentResult = null;

            var result = new PathResult(PathingStatus.Failed, null, 0, request)
            {
                errorInfo = string.Concat(e.Message, Environment.NewLine, e.StackTrace)
            };

            request.Complete(result);
        }

        private void FixupGoal(IPathRequest request)
        {
            var unitProps = request.requesterProperties;
            var requesterRadius = request.requesterProperties.radius;
            var actualGoal = request.to;

            var halfCell = _goal.parent.cellSize * 0.5f;

            var dx = actualGoal.x - _goal.position.x;
            var dz = actualGoal.z - _goal.position.z;

            var overlapLeft = (requesterRadius - dx) - halfCell;
            var overlapRight = (dx + requesterRadius) - halfCell;
            var overlapTop = (dz + requesterRadius) - halfCell;
            var overlapBottom = (requesterRadius - dz) - halfCell;

            var adjX = 0.0f;
            var adjZ = 0.0f;

            if (overlapLeft > 0.0f && !ValidateGoalNeighbour(unitProps, -1, 0))
            {
                adjX = -overlapLeft;
            }
            else if (overlapRight > 0.0f && !ValidateGoalNeighbour(unitProps, 1, 0))
            {
                adjX = overlapRight;
            }

            if (overlapTop > 0.0f && !ValidateGoalNeighbour(unitProps, 0, 1))
            {
                adjZ = overlapTop;
            }
            else if (overlapBottom > 0.0f && !ValidateGoalNeighbour(unitProps, 0, -1))
            {
                adjZ = -overlapBottom;
            }

            //If we do overlap direct neighbours but they are free we have to ensure that the diagonal is also free.
            if ((adjX == 0.0f) && (adjZ == 0.0f))
            {
                if ((overlapLeft > 0.0f) && (overlapTop > 0.0f) && !ValidateGoalNeighbour(unitProps, -1, 1))
                {
                    adjX = -overlapLeft;
                    adjZ = overlapTop;
                }
                else if ((overlapLeft > 0.0f) && (overlapBottom > 0.0f) && !ValidateGoalNeighbour(unitProps, -1, -1))
                {
                    adjX = -overlapLeft;
                    adjZ = -overlapBottom;
                }
                else if ((overlapRight > 0.0f) && (overlapTop > 0.0f) && !ValidateGoalNeighbour(unitProps, 1, 1))
                {
                    adjX = overlapRight;
                    adjZ = overlapTop;
                }
                else if ((overlapRight > 0.0f) && (overlapBottom > 0.0f) && !ValidateGoalNeighbour(unitProps, 1, -1))
                {
                    adjX = overlapRight;
                    adjZ = -overlapBottom;
                }
            }

            if ((adjX != 0.0f) || (adjZ != 0.0f))
            {
                request.to = new Vector3(actualGoal.x - adjX, actualGoal.y, actualGoal.z - adjZ);
            }
        }

        private bool ValidateGoalNeighbour(IUnitProperties unitProps, int dx, int dz)
        {
            var c = _goal.GetNeighbour(dx, dz);
            return ((c == null) || c.IsWalkableFrom(_goal, unitProps));
        }

        /// <summary>
        /// Processes the next node.
        /// </summary>
        /// <returns>The current pathing status</returns>
        protected abstract PathingStatus ProcessNext();

        /// <summary>
        /// Called when a request is about to be processed.
        /// </summary>
        /// <param name="start">The start node.</param>
        /// <param name="actualStart">The actual start node in case actual start is blocked.</param>
        protected virtual void OnStart(IPathNode start, IPathNode actualStart)
        {
        }

        private class SegmentRequest : IPathRequest
        {
            private IPathRequest _actualRequest;
            private Vector3 _actualFrom;
            private Vector3 _actualTo;
            private int _viaIdx;

            public Vector3 from
            {
                get { return _actualFrom; }
                set { _actualFrom = value; }
            }

            public Vector3 to
            {
                get { return _actualTo; }
                set { _actualTo = value; }
            }

            public Vector3[] via
            {
                get { return null; }
                set {  /* NOOP */ }
            }

            public bool isRunning
            {
                get { return _actualRequest != null; }
            }

            public INeedPath requester
            {
                get { return _actualRequest.requester; }
                set { throw new InvalidOperationException(); }
            }

            public IUnitProperties requesterProperties
            {
                get { return _actualRequest.requesterProperties; }
                set { _actualRequest.requesterProperties = value; }
            }

            public IPathFinderOptions pathFinderOptions
            {
                get { return _actualRequest.pathFinderOptions; }
                set { _actualRequest.pathFinderOptions = value; }
            }

            public RequestType type
            {
                get { return _actualRequest.type; }
                set { _actualRequest.type = value; }
            }

            public float timeStamp
            {
                get;
                set;
            }

            public bool isValid
            {
                get { return _actualRequest.isValid; }
            }

            public bool hasDecayed
            {
                get { return _actualRequest.hasDecayed; }
                set { _actualRequest.hasDecayed = value; }
            }

            public object customData
            {
                get { return _actualRequest.customData; }
                set { _actualRequest.customData = value; }
            }

            public void Complete(PathResult result)
            {
                throw new InvalidOperationException();
            }

            public void Start(IPathRequest req)
            {
                _actualRequest = req;
                _actualFrom = req.from;

                if (req.via != null && req.via.Length > 0)
                {
                    _viaIdx = 0;
                    _actualTo = req.via[0];
                }
                else
                {
                    _viaIdx = -1;
                    _actualTo = req.to;
                }
            }

            public bool MoveNext()
            {
                if (_viaIdx < 0)
                {
                    _actualRequest.to = _actualTo;
                    return false;
                }

                var via = _actualRequest.via;
                via[_viaIdx++] = _actualTo;

                _actualFrom = _actualTo;

                if (_viaIdx == via.Length)
                {
                    _actualTo = _actualRequest.to;
                    _viaIdx = -1;
                }
                else
                {
                    _actualTo = via[_viaIdx];
                }

                return true;
            }

            public Vector3[] GetPendingWaypoints()
            {
                if (_viaIdx < 0)
                {
                    return new Vector3[] { _actualRequest.to };
                }

                var via = _actualRequest.via;
                var remaining = (via.Length - _viaIdx);
                var arr = new Vector3[remaining + 1];
                Array.Copy(via, _viaIdx, arr, 0, remaining);
                arr[remaining] = _actualRequest.to;

                return arr;
            }

            public void Clear()
            {
                _actualRequest = null;
            }
        }

        private class SafeIterator : IEnumerator
        {
            private PathingEngineBase _engine;
            private IEnumerator _innerIter;
            private IPathRequest _request;

            public SafeIterator(PathingEngineBase engine)
            {
                _engine = engine;
            }

            //This is not used for anything
            public object Current
            {
                get { return _innerIter.Current; }
            }

            public bool MoveNext()
            {
                try
                {
                    return _innerIter.MoveNext();
                }
                catch (Exception e)
                {
                    _engine.FailRequest(_request, e);
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public void Init(IPathRequest request)
            {
                _request = request;
                _innerIter = _engine.ProcessRequestCoroutineInternal(request);
            }
        }
    }
}
