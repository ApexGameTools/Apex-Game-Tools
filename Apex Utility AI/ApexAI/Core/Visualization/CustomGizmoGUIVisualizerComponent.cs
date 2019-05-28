/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using UnityEngine;

    /// <summary>
    /// Base class for custom visualizer components that draws GUI and Gizmos
    /// </summary>
    /// <typeparam name="T">The type that this visualizer visualizes.</typeparam>
    /// <typeparam name="TData">The type of the data to be visualized.</typeparam>
    public abstract class CustomGizmoGUIVisualizerComponent<T, TData> : CustomVisualizerComponent<T, TData> where T : class
    {
        /// <summary>
        /// Should we draw gizmos
        /// </summary>
        public bool drawGizmos = true;

        /// <summary>
        /// Should we draw GUI
        /// </summary>
        public bool drawGUI = true;

        /// <summary>
        /// Draw GUI using the provided data.
        /// </summary>
        /// <param name="data">The data to visualize.</param>
        protected abstract void DrawGUI(TData data);

        /// <summary>
        /// Draw gizmos using the provided data.
        /// </summary>
        /// <param name="data">The data to visualize.</param>
        protected abstract void DrawGizmos(TData data);

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !drawGizmos || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGizmoData);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !drawGUI || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGUIData);
        }

        private void DrawGizmoData(IAIContext context)
        {
            TData data;
            if (_data.TryGetValue(context, out data))
            {
                DrawGizmos(data);
            }
        }

        private void DrawGUIData(IAIContext context)
        {
            TData data;
            if (_data.TryGetValue(context, out data))
            {
                DrawGUI(data);
            }
        }
    }
}
