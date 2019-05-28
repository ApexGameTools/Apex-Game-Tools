/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for custom visualizer components
    /// </summary>
    /// <typeparam name="T">The type that this visualizer visualizes.</typeparam>
    /// <typeparam name="TData">The type of the data to be visualized.</typeparam>
    /// <seealso cref="Apex.AI.Visualization.ICustomVisualizer" />
    public abstract class CustomVisualizerComponent<T, TData> : ContextVisualizerComponent, ICustomVisualizer where T : class
    {
        /// <summary>
        /// The data registered for visualization
        /// </summary>
        protected Dictionary<IAIContext, TData> _data;

        /// <summary>
        /// Gets or sets a value indicating whether to register this visualizer for derived types of <typeparamref name="T"/>.
        /// </summary>
        protected bool registerForDerivedTypes
        {
            get;
            set;
        }

        /// <summary>
        /// Unity Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _data = new Dictionary<IAIContext, TData>();
        }

        /// <summary>
        /// Unity OnEnable
        /// </summary>
        protected virtual void OnEnable()
        {
            VisualizationManager.RegisterVisualizer<T>(this, this.registerForDerivedTypes);
        }

        /// <summary>
        /// Unity OnDisable
        /// </summary>
        protected virtual void OnDisable()
        {
            VisualizationManager.UnregisterVisualizer<T>(this.registerForDerivedTypes);
        }

        /// <summary>
        /// Called after an entity of the type associated with this visualizer has been executed in the AI, e.g. an <see cref="IAction" />.
        /// </summary>
        /// <param name="aiEntity">The AI entity that has just executed.</param>
        /// <param name="context">The context.</param>
        /// <param name="aiId">The ID of the AI the entity belongs to.</param>
        void ICustomVisualizer.EntityUpdate(object aiEntity, IAIContext context, Guid aiId)
        {
            _data[context] = GetDataForVisualization(aiEntity as T, context, aiId);
        }

        /// <summary>
        /// Called after an entity of the type associated with this visualizer has been executed in the AI, e.g. an <see cref="IAction" />.
        /// </summary>
        /// <param name="aiEntity">The AI entity that has just executed.</param>
        /// <param name="context">The context.</param>
        /// <param name="aiId">The ID of the AI the entity belongs to.</param>
        /// <returns>The data that should be visualized.</returns>
        protected abstract TData GetDataForVisualization(T aiEntity, IAIContext context, Guid aiId);
    }
}
