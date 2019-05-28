/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using PathFinding;
    using PathFinding.MoveCost;
    using Services;
    using Units;
    using UnityEngine;
    using Utilities;
    using WorldGeometry;

#if UNITY_EDITOR
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Because its a lame rule.")]
#endif
    [AddComponentMenu("Apex/Game World/Debugging/Path Finder Visualizer", 1014)]
    public class PathFinderVisualizer : MonoBehaviour, INeedPath
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

        [Tooltip("The unit used for navigation settings.")]
        public GameObject unit;

        [Tooltip("What cost info to display for each cell.")]
        public CostDiplay costInfo = CostDiplay.None;

        [Tooltip("Controls whether the visualization is done step-by-step or in full.")]
        public bool stepByStep = true;

        [Tooltip("Controls whether an instructions box is displayed.")]
        public bool showInstructions = true;

        private Styles _styles;
        private IVisualizedPathEngine _engine;
        private IEnumerator _pathEnumerator;
        private BasicPathRequest _req;
        private IUnitFacade _unit;
        private float _cellSize;
        private Vector3 _gizmoSize;

        public enum CostDiplay
        {
            None,
            MoveCost,
            Heuristics,
            Total
        }

        private void Awake()
        {
            Init();
        }

        public void Reset()
        {
            ResetEnumerator();
            Init();
        }

        private void Init()
        {
            if (unit == null)
            {
                Debug.LogWarning("The Path Finder Visualizer must have a unit set.");
                return;
            }

            _unit = this.unit.GetUnitFacade();

            var g = GridManager.instance.GetGrid(_unit.position);
            _cellSize = g.cellSize;
            _gizmoSize = new Vector3(_cellSize, 0.2f, _cellSize);

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
            if (this.engineType == PathingEngineType.Astar)
            {
                _engine = new VisualizedAStar(this.initialHeapSize, moveCostProvider, GameServices.cellCostStrategy, new PathSmoother(), preProcessors);
            }
            else
            {
                _engine = new VisualizedJPS(this.initialHeapSize, moveCostProvider, GameServices.cellCostStrategy, new PathSmoother(), preProcessors);
            }

            var tmp = new BasicPathRequest
            {
                requester = this,
                requesterProperties = _unit,
                pathFinderOptions = _unit.pathFinderOptions
            };

            if (_req != null)
            {
                tmp.from = _req.from;
                tmp.to = _req.to;
            }

            _req = tmp;
        }

        private void Update()
        {
            RaycastHit hit;

            if (Input.GetMouseButtonUp(0))
            {
                ResetEnumerator();
                UnityServices.mainCamera.ScreenToLayerHit(Input.mousePosition, Layers.terrain, 1000.0f, out hit);
                _req.from = hit.point;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                ResetEnumerator();
                UnityServices.mainCamera.ScreenToLayerHit(Input.mousePosition, Layers.terrain, 1000.0f, out hit);
                _req.to = hit.point;
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                ResetEnumerator();
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                if (_req == null)
                {
                    return;
                }

                if (_pathEnumerator == null)
                {
                    _pathEnumerator = _engine.ProcessRequestCoroutine(_req);
                }

                if (!_pathEnumerator.MoveNext())
                {
                    ResetEnumerator();
                }
                else if (!stepByStep)
                {
                    while (_pathEnumerator.MoveNext())
                    {
                        /* noop */
                    }
                }
            }
        }

        private void ResetEnumerator()
        {
            if (_pathEnumerator != null)
            {
                while (_pathEnumerator.MoveNext())
                {
                    /* noop */
                }

                _pathEnumerator = null;
                _engine.Reset();
            }
        }

        private Vector3 GetPos(IPathNode n)
        {
            if (n is PortalCell)
            {
                return n.predecessor.position;
            }

            return n.position;
        }

        private void OnDrawGizmos()
        {
            if (_engine == null)
            {
                return;
            }

            var nodes = _engine.expandedSet;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                var pos = GetPos(n);

                Gizmos.color = Color.green;
                if (n.isClosed)
                {
                    if (n is PortalCell)
                    {
                        Gizmos.color = Color.magenta;
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                    }
                }
                else if (n is PortalCell)
                {
                    continue;
                }

                Gizmos.DrawCube(pos, _gizmoSize);
            }

            Gizmos.color = Color.blue;
            var cur = _engine.currentNode;
            if (cur != null)
            {
                if (!(cur is PortalCell))
                {
                    Gizmos.DrawCube(cur.position, _gizmoSize);
                }

                while (cur.predecessor != null)
                {
                    var pos = GetPos(cur);

                    Gizmos.DrawLine(pos, GetPos(cur.predecessor));
                    cur = cur.predecessor;
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_req.from, 0.25f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_req.to, 0.25f);
        }

        private void OnGUI()
        {
            if (_styles == null)
            {
                _styles = new Styles();
            }

            if (costInfo != CostDiplay.None)
            {
                if (Camera.main == null)
                {
                    return;
                }

                var nodes = _engine.expandedSet;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var n = nodes[i];
                    if ((n == null) || (n is PortalCell))
                    {
                        continue;
                    }

                    int cost;
                    switch (costInfo)
                    {
                        case CostDiplay.MoveCost:
                        {
                            cost = n.g;
                            break;
                        }

                        case CostDiplay.Heuristics:
                        {
                            cost = n.h;
                            break;
                        }

                        default:
                        {
                            cost = n.f;
                            break;
                        }
                    }

                    var pos = Camera.main.WorldToScreenPoint(n.position);
                    pos.y = Screen.height - pos.y;
                    GUI.color = Color.black;
                    GUI.Label(new Rect(pos.x - 10f, pos.y - 6f, 50f, 20f), cost.ToString());
                }
            }

            if (showInstructions)
            {
                float width = 300f;
                float height = Screen.height * 0.9f;

                GUILayout.BeginArea(new Rect(5f, (Screen.height / 2f) - (height / 2f), width, height), GUI.skin.window);
                GUILayout.BeginVertical();
                GUILayout.Label("Path Finder Visualization", _styles.headerStyle);
                GUILayout.Label("Blue Squares represent the node being processed.");
                GUILayout.Label("Yellow Squares represent the nodes that have been processed (closed).");
                GUILayout.Label("Green Squares represent expanded nodes yet to be processed.");
                GUILayout.Label("Purple Squares represent nodes from which a portal is crossed.");
                GUILayout.Label("\nLeft click to set the Starting Point.\n\nRight click to set the End Point.\n\nPress 'Space' to visualize.\n\nPress 'R' to reset.");

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        public void ConsumePathResult(PathResult result)
        {
            Debug.Log("Pathing complete, result: " + result.status);
            if (result.status == PathingStatus.Failed)
            {
                Debug.Log(result.errorInfo);
            }
            else
            {
                Debug.Log("Path length: " + result.path.length);
            }
        }

        private interface IVisualizedPathEngine : IPathingEngine
        {
            List<IPathNode> expandedSet
            {
                get;
            }

            IPathNode currentNode
            {
                get;
            }

            void Reset();
        }

        private class Styles
        {
            public GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
        }

        private class VisualizedAStar : PathingAStar, IVisualizedPathEngine
        {
            public VisualizedAStar(int heapInitialSize, IMoveCost moveCostProvider, ICellCostStrategy cellCostStrategy, ISmoothPaths pathSmoother, IRequestPreProcessor[] requestPreprocessors)
            : base(heapInitialSize, moveCostProvider, cellCostStrategy, pathSmoother, requestPreprocessors)
            {
            }

            public List<IPathNode> expandedSet
            {
                get { return _expandedSet; }
            }

            public IPathNode currentNode
            {
                get { return _current; }
            }

            public void Reset()
            {
                _expandedSet.Apply(c => c.g = 0);
                _expandedSet.Clear();
                _current = null;
            }
        }

        private class VisualizedJPS : PathingJumpPointSearch, IVisualizedPathEngine
        {
            public VisualizedJPS(int heapInitialSize, IMoveCost moveCostProvider, ICellCostStrategy cellCostStrategy, ISmoothPaths pathSmoother, IRequestPreProcessor[] requestPreprocessors)
            : base(heapInitialSize, moveCostProvider, cellCostStrategy, pathSmoother, requestPreprocessors)
            {
            }

            public List<IPathNode> expandedSet
            {
                get { return _expandedSet; }
            }

            public IPathNode currentNode
            {
                get { return _current; }
            }

            public void Reset()
            {
                _expandedSet.Apply(c => c.g = 0);
                _expandedSet.Clear();
                _current = null;
            }
        }
    }
}
