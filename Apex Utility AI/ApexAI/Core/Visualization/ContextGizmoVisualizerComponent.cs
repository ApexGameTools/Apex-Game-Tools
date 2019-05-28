/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using UnityEngine;

    /// <summary>
    /// Possible base class for scene visualizers, that facilitate drawing gizmos to visualize context state.
    /// </summary>
    /// <seealso cref="Apex.AI.Visualization.ContextGUIVisualizerComponent" />
    /// <seealso cref="Apex.AI.Visualization.ContextGizmoGUIVisualizerComponent" />
    public abstract class ContextGizmoVisualizerComponent : ContextVisualizerComponent
    {
        /// <summary>
        /// Draw gizmos using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawGizmos(IAIContext context);

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGizmos);
        }
    }
}
