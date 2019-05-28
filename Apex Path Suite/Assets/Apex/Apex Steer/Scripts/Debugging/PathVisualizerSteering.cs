/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Debugging
{
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Visualization component that draws gizmos to represent a group's path and optionally its vector field.
    /// </summary>
    public partial class PathVisualizer
    {
        /// <summary>
        /// Whether to debug draw the vector field (true) or not (false).
        /// </summary>
        public bool drawVectorField = true;

        /// <summary>
        /// Draws the actual visualization.
        /// </summary>
        partial void DrawVisualizationPartial()
        {
            if (_unit == null)
            {
                // if the unit facade is not available, return
                return;
            }

            var group = _unit.transientGroup as DefaultSteeringTransientUnitGroup;
            if (group == null || _unit != group.modelUnit)
            {
                // if the unit does not have a group or is not the group's model unit, return
                return;
            }

            var vectorField = group.vectorField;
            if (vectorField == null || vectorField.currentPath == null || vectorField.currentPath.count == 0)
            {
                // if the group has no valid vector field, return
                return;
            }

            if (drawVectorField)
            {
                group.vectorField.DebugRender();
            }

            var path = vectorField.currentPath;
            Vector3 heightAdj = new Vector3(0f, 0.2f, 0f);
            Gizmos.color = this.routeColor;

            // draw the path
            var prev = path.Peek().position;
            foreach (var n in path)
            {
                if (n is IPortalNode)
                {
                    // do not draw portal nodes
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

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="pathViz">The component to clone from.</param>
        public void CloneFrom(PathVisualizer pathViz)
        {
            this.drawAlways = pathViz.drawAlways;
            this.showSegmentMarkers = pathViz.showSegmentMarkers;
            this.drawVectorField = pathViz.drawVectorField;

            this.routeColor = pathViz.routeColor;
            this.waypointColor = pathViz.waypointColor;
        }
    }
}