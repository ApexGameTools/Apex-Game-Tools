/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    /// <summary>
    /// Generic version of <see cref="ContextGizmoVisualizerComponent"/>.
    /// </summary>
    /// <typeparam name="T">The context type.</typeparam>
    /// <seealso cref="Apex.AI.Visualization.ContextGizmoVisualizerComponent" />
    public abstract class ContextGizmoVisualizerComponent<T> : ContextGizmoVisualizerComponent where T : IAIContext
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
    }
}
