/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using System.Linq;
    using Apex.PathFinding.MoveCost;
    using Apex.Services;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Component for configuring the <see cref="PathService"/>
    /// </summary>
    [AddComponentMenu("Apex/Game World/Path Service", 1038)]
    [ApexComponent("Game World")]
    public sealed class PathServiceComponent : SingleInstanceComponent<PathServiceComponent>
    {
        /// <summary>
        /// The pathing engine to use
        /// </summary>
        [Tooltip("The pathing engine to use.")]
        public PathingEngineType engineType;

        /// <summary>
        /// The move cost provider to use
        /// </summary>
        [Tooltip("The algorithm used for calculating move costs.")]
        public CostProviderType moveCost = CostProviderType.Diagonal;

        /// <summary>
        /// The initial heap size. The optimal size depends on the size of the grid, and there is no static approximate factor, since the heap use percentage related to grid size diminishes as the grid gets bigger.
        /// </summary>
        [MinCheck(50, tooltip = "Memory allocation optimization. You only need to change this if you get warnings that ask you to do so.")]
        public int initialHeapSize = 100;

        /// <summary>
        /// Whether the path finding should be done asynchronously, i.e. on a separate thread.
        /// </summary>
        [Tooltip("Whether the path finding will run in a separate thread.")]
        public bool runAsync = true;

        /// <summary>
        /// Use thread pool for async request if available.
        /// </summary>
        [Tooltip("Whether to use a thread pool if available instead of a dedicated thread. The recommendation is to use a dedicated thread.")]
        public bool useThreadPoolForAsyncOperations = false;

        /// <summary>
        /// The maximum milliseconds per frame to use for path finding
        /// </summary>
        [MinCheck(1, tooltip = "The maximum number of milliseconds to user for path finding per frame when the path finder runs in the main thread.")]
        public int maxMillisecondsPerFrame = 5;

        private PathService _pathService;

        /// <summary>
        /// Called on awake.
        /// </summary>
        protected sealed override void OnAwake()
        {
            //Determine the cost and path smoothing providers to use
            ISmoothPaths pathSmoother;
            var pathSmoothProvider = this.As<IPathSmootherFactory>();
            if (pathSmoothProvider == null)
            {
                pathSmoother = new PathSmoother();
            }
            else
            {
                pathSmoother = pathSmoothProvider.CreateSmoother();
            }

            IMoveCost moveCostProvider;
            if (this.moveCost == CostProviderType.Custom)
            {
                var moveCostProviderFactory = this.As<IMoveCostFactory>();
                if (moveCostProviderFactory == null)
                {
                    moveCostProvider = new DiagonalDistance(10);
                    Debug.LogWarning("Path Service Component: Cost Provider set to Custom but no Cost Provider Factory found!.");
                }
                else
                {
                    moveCostProvider = moveCostProviderFactory.CreateMoveCostProvider();
                }
            }
            else
            {
                switch (this.moveCost)
                {
                    case CostProviderType.Euclidean:
                    {
                        moveCostProvider = new EuclideanDistance(10);
                        break;
                    }

                    case CostProviderType.Cardinal:
                    {
                        moveCostProvider = new CardinalDistance(10);
                        break;
                    }

                    case CostProviderType.Manhattan:
                    {
                        moveCostProvider = new ManhattanDistance(10);
                        break;
                    }

                    default:
                    {
                        moveCostProvider = new DiagonalDistance(10);
                        break;
                    }
                }
            }

            var preProcessors = this.GetComponents(typeof(IRequestPreProcessor)).Cast<IRequestPreProcessor>().OrderByDescending(p => p.priority).ToArray();

            //Setup the pathing engine to use
            IPathingEngine engine;
            if (this.engineType == PathingEngineType.Astar)
            {
                engine = new PathingAStar(this.initialHeapSize, moveCostProvider, GameServices.cellCostStrategy, pathSmoother, preProcessors);
            }
            else
            {
                engine = new PathingJumpPointSearch(this.initialHeapSize, moveCostProvider, GameServices.cellCostStrategy, pathSmoother, preProcessors);
            }

            _pathService = new PathService(engine, new ThreadFactory(), this.useThreadPoolForAsyncOperations);
            _pathService.runAsync = this.runAsync;
            GameServices.pathService = _pathService;
        }

        private void Start()
        {
            _pathService.OnAsyncFailed = OnAsyncFailed;

            if (!this.runAsync)
            {
                StartCoroutine(_pathService.ProcessRequests(maxMillisecondsPerFrame));
            }
        }

        /// <summary>
        /// Called when destroyed.
        /// </summary>
        protected sealed override void OnDestroy()
        {
            if (_pathService != null)
            {
                _pathService.OnAsyncFailed = null;
                _pathService.Dispose();
            }

            base.OnDestroy();
        }

        private void OnAsyncFailed()
        {
            this.runAsync = false;
            _pathService.runAsync = false;
            StartCoroutine(_pathService.ProcessRequests(maxMillisecondsPerFrame));
        }
    }
}
