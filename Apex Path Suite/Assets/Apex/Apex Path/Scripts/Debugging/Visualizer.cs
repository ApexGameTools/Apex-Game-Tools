/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using UnityEngine;

    /// <summary>
    /// Base class for standalone visualizers.
    /// </summary>
    public abstract class Visualizer : MonoBehaviour
    {
        /// <summary>
        /// Whether to always draw the visualization gizmos, even when the gameobject is not selected.
        /// </summary>
        [Tooltip("Whether to always draw the visualization gizmos, even when the gameobject is not selected.")]
        public bool drawAlways;

        /// <summary>
        /// Forces a redraw
        /// </summary>
        public void Refresh()
        {
            DrawVisualization();
        }

        /// <summary>
        /// Draws the actual visualization.
        /// </summary>
        protected abstract void DrawVisualization();

        /// <summary>
        /// Called on start
        /// </summary>
        protected virtual void Start()
        {
            /* NOOP but required to be able to disable */
        }

        private void OnDrawGizmos()
        {
            if (drawAlways && this.enabled)
            {
                DrawVisualization();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawAlways && this.enabled)
            {
                DrawVisualization();
            }
        }
    }
}
