/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using UnityEngine;

    /// <summary>
    /// Possible base class for scene visualizers, that facilitate drawing gizmos and GUI to visualize context state.
    /// </summary>
    /// <seealso cref="Apex.AI.Visualization.ContextGizmoVisualizerComponent" />
    /// <seealso cref="Apex.AI.Visualization.ContextGizmoGUIVisualizerComponent" />
    public abstract class ContextGUIVisualizerComponent : ContextVisualizerComponent
    {
        /// <summary>
        /// Draw GUI using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawGUI(IAIContext context);

        private void OnGUI()
        {
            if (!Application.isPlaying || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGUI);
        }
    }
}
