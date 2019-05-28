/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System;

    /// <summary>
    /// Interface for custom visualizers that can be registered with the <see cref="VisualizationManager"/> to show additional visualization for specific AI entity types, typically in the form of Gizmos.
    /// </summary>
    public interface ICustomVisualizer
    {
        /// <summary>
        /// Called after an entity of the type associated with this visualizer has been executed in the AI, e.g. an <see cref="IAction"/>.
        /// </summary>
        /// <param name="aiEntity">The AI entity that has just executed.</param>
        /// <param name="context">The context.</param>
        /// <param name="aiId">The ID of the AI the entity belongs to.</param>
        void EntityUpdate(object aiEntity, IAIContext context, Guid aiId);
    }
}
