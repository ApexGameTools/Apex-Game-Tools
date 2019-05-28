/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using UnityEngine;

    /// <summary>
    /// Base class for custom visualizer components that draws Gizmos.
    /// </summary>
    /// <typeparam name="T">The type that this visualizer visualizes.</typeparam>
    /// <typeparam name="TData">The type of the data to be visualized.</typeparam>
    public abstract class CustomGizmoVisualizerComponent<T, TData> : CustomVisualizerComponent<T, TData> where T : class
    {
        /// <summary>
        /// Draw gizmos using the provided data.
        /// </summary>
        /// <param name="data">The data to visualize.</param>
        protected abstract void DrawGizmos(TData data);

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGizmoData);
        }

        private void DrawGizmoData(IAIContext context)
        {
            TData data;
            if (_data.TryGetValue(context, out data))
            {
                DrawGizmos(data);
            }
        }
    }
}
