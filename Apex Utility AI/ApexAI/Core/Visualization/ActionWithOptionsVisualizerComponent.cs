/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A specialized visualizer that handles visualization of <see cref="ScoredOption{TOption}"/> for a <see cref="ActionWithOptions{TOption}"/>
    /// </summary>
    /// <typeparam name="T">The concrete ActionWithOptions type</typeparam>
    /// <typeparam name="TOption">The type of the options.</typeparam>
    /// <seealso cref="Apex.AI.Visualization.CustomGizmoGUIVisualizerComponent{T, TData}" />
    public abstract class ActionWithOptionsVisualizerComponent<T, TOption> : CustomGizmoGUIVisualizerComponent<T, IList<ScoredOption<TOption>>> where T : ActionWithOptions<TOption>
    {
        private Dictionary<IAIContext, List<ScoredOption<TOption>>> _buffers = new Dictionary<IAIContext, List<ScoredOption<TOption>>>();

        /// <summary>
        /// Called after an entity of the type associated with this visualizer has been executed in the AI, e.g. an <see cref="T:Apex.AI.IAction" />.
        /// </summary>
        /// <param name="aiEntity">The AI entity that has just executed.</param>
        /// <param name="context">The context.</param>
        /// <param name="aiId">The ID of the AI the entity belongs to.</param>
        /// <returns>
        /// The data that should be visualized.
        /// </returns>
        protected override IList<ScoredOption<TOption>> GetDataForVisualization(T aiEntity, IAIContext context, Guid aiId)
        {
            var options = GetOptions(context);

            List<ScoredOption<TOption>> buffer;
            if (!_buffers.TryGetValue(context, out buffer))
            {
                _buffers[context] = buffer = new List<ScoredOption<TOption>>(options.Count);
            }
            else
            {
                buffer.Clear();
                buffer.EnsureCapacity(options.Count);
            }
            
            aiEntity.GetAllScores(context, options, buffer);
            return buffer;
        }

        /// <summary>
        /// Gets the options to score and visualize.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The options to score and visualize</returns>
        protected abstract IList<TOption> GetOptions(IAIContext context);
    }
}
