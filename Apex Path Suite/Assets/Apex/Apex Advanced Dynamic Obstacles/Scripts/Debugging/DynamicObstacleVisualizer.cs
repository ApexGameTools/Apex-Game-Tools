/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Visualization component that draws gizmos to represent a Dynamic Obstacles bounding polygon.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Debugging/Dynamic Obstacle Visualizer", 1012)]
    [ApexComponent("Debugging")]
    public class DynamicObstacleVisualizer : Visualizer
    {
        /// <summary>
        /// Controls whether the visualizer draws all dynamic obstacles in the scene or only those on the same GameObject as the visualizer.
        /// </summary>
        public bool drawAllObstacles = false;

        /// <summary>
        /// The outline color
        /// </summary>
        public Color outlineColor = new Color(195f / 255f, 214f / 255f, 53f / 255f);

        /// <summary>
        /// Draws the actual visualization.
        /// </summary>
        protected override void DrawVisualization()
        {
            Gizmos.color = this.outlineColor;

            var obstacles = this.drawAllObstacles ? FindObjectsOfType<DynamicObstacle>() : GetComponents<DynamicObstacle>();
            foreach (var o in obstacles)
            {
                o.RenderVisualization();
            }
        }
    }
}
