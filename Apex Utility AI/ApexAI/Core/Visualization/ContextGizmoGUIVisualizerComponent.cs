/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using UnityEngine;

    /// <summary>
    /// Possible base class for scene visualizers, that facilitate drawing gizmos and GUI to visualize context state.
    /// </summary>
    /// <seealso cref="Apex.AI.Visualization.ContextGizmoVisualizerComponent" />
    /// <seealso cref="Apex.AI.Visualization.ContextGUIVisualizerComponent" />
    public abstract class ContextGizmoGUIVisualizerComponent : ContextVisualizerComponent
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
        /// Draw GUI using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawGUI(IAIContext context);

        /// <summary>
        /// Draw gizmos using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawGizmos(IAIContext context);

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !drawGizmos || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGizmos);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !drawGUI || !this.enabled)
            {
                return;
            }

            DoDraw(DrawGUI);
        }
    }
}
