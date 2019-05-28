/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Visualization component that draws gizmos to represent a moving unit's current path.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Debugging/Path Visualizer", 1024)]
    [ApexComponent("Debugging")]
    public partial class PathVisualizer : Visualizer
    {
        /// <summary>
        /// The route color
        /// </summary>
        public Color routeColor = new Color(148f / 255f, 214f / 255f, 53f / 255f);

        /// <summary>
        /// The way point color
        /// </summary>
        public Color waypointColor = new Color(0f, 150f / 255f, 211f / 255f);

        /// <summary>
        /// Whether to show segment markers
        /// </summary>
        public bool showSegmentMarkers = false;

        private IUnitFacade _unit;

        /// <summary>
        /// Called on start
        /// </summary>
        protected override void Start()
        {
            _unit = this.GetUnitFacade();
        }

        /// <summary>
        /// Partial method for draw visualization.
        /// </summary>
        partial void DrawVisualizationPartial();

        /// <summary>
        /// Draws the actual visualization.
        /// </summary>
        protected override void DrawVisualization()
        {
            DrawVisualizationPartial();

            if (_unit == null || !_unit.isMovable)
            {
                return;
            }

            Vector3 heightAdj = new Vector3(0.0f, 0.2f, 0.0f);
            Gizmos.color = this.routeColor;

            var prev = _unit.position;
            if (_unit.nextNodePosition.HasValue)
            {
                prev = _unit.nextNodePosition.Value;
                Gizmos.DrawLine(_unit.position + heightAdj, prev + heightAdj);
            }

            if (_unit.currentPath != null)
            {
                foreach (var n in _unit.currentPath)
                {
                    if (n is IPortalNode)
                    {
                        continue;
                    }

                    if (showSegmentMarkers)
                    {
                        Gizmos.DrawSphere(prev, 0.2f);
                    }

                    Gizmos.DrawLine(prev + heightAdj, n.position + heightAdj);
                    prev = n.position;
                }
            }

            Gizmos.color = this.waypointColor;
            if (_unit.currentWaypoints != null)
            {
                heightAdj.y = 1.0f;

                foreach (var wp in _unit.currentWaypoints)
                {
                    var pinHead = wp + heightAdj;
                    Gizmos.DrawLine(wp, pinHead);
                    Gizmos.DrawSphere(pinHead, 0.3f);
                }
            }
        }
    }
}
