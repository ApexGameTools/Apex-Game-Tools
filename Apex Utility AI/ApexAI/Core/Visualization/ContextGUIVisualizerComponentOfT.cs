/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    /// <summary>
    /// Generic version of <see cref="ContextGUIVisualizerComponent"/>.
    /// </summary>
    /// <typeparam name="T">The context type.</typeparam>
    /// <seealso cref="Apex.AI.Visualization.ContextGUIVisualizerComponent" />
    public abstract class ContextGUIVisualizerComponent<T> : ContextGUIVisualizerComponent where T : IAIContext
    {
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
