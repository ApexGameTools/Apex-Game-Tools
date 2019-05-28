/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using UnityEngine;

    /// <summary>
    /// Base class for custom visualizer components that draws GUI.
    /// </summary>
    /// <typeparam name="T">The type that this visualizer visualizes.</typeparam>
    /// <typeparam name="TData">The type of the data to be visualized.</typeparam>
    public abstract class CustomGUIVisualizerComponent<T, TData> : CustomVisualizerComponent<T, TData> where T : class
    {
        /// <summary>
        /// Draw GUI using the provided data.
        /// </summary>
        /// <param name="data">The data to visualize.</param>
        protected abstract void DrawGUI(TData data);

        private void OnGUI()
        {
            if (!Application.isPlaying || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGUIData);
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
