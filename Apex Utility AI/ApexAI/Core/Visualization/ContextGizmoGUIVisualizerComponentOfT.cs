/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    /// <summary>
    /// Generic version of <see cref="ContextGizmoGUIVisualizerComponent"/>.
    /// </summary>
    /// <typeparam name="T">The context type.</typeparam>
    /// <seealso cref="Apex.AI.Visualization.ContextGizmoGUIVisualizerComponent" />
    public abstract class ContextGizmoGUIVisualizerComponent<T> : ContextGizmoGUIVisualizerComponent where T : IAIContext
    {
        /// <summary>
        /// Draw gizmos using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected sealed override void DrawGizmos(IAIContext context)
        {
            DrawGizmos((T)context);
        }

        /// <summary>
        /// Draw gizmos using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawGizmos(T context);

        /// <summary>
        /// Draw GUI using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected sealed override void DrawGUI(IAIContext context)
        {
            DrawGUI((T)context);
        }

        /// <summary>
        /// Draw GUI using the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawGUI(T context);
    }
}
