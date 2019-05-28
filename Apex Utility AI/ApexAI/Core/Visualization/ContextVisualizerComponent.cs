/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Possible base class for scene visualizers, that facilitate drawing visualizations of context state.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public abstract class ContextVisualizerComponent : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        internal string relevantAIId;

        [SerializeField, HideInInspector]
        internal SceneVisualizationMode mode = SceneVisualizationMode.SingleSelectedGameObject;

        private Guid _relevantAIGuid;

        /// <summary>
        /// If the Mode of the visualizer is set to <see cref="SceneVisualizationMode.Custom"/>, implement this to return the relevant contexts to visualize.
        /// </summary>
        /// <param name="contextsBuffer">The contexts buffer to be filled with relevant contexts for visualization.</param>
        /// <param name="relevantAIId">The relevant AI identifier, or <see cref="Guid.Empty"/> if no specific AI is targeted..</param>
        protected virtual void GetContextsToVisualize(List<IAIContext> contextsBuffer, Guid relevantAIId)
        {
            /* NOOP */
        }

        /// <summary>
        /// The Unity Awake message. If needed this can be overridden, just be sure to call the base.
        /// </summary>
        protected virtual void Awake()
        {
            _relevantAIGuid = string.IsNullOrEmpty(this.relevantAIId) ? Guid.Empty : new Guid(this.relevantAIId);
        }

        private void OnEnable()
        {
            /* NOOP, just to enable disable */
        }

        /// <summary>
        /// Draws stuff
        /// </summary>
        protected void DoDraw(Action<IAIContext> drawer)
        {
            switch (mode)
            {
                case SceneVisualizationMode.SingleSelectedGameObject:
                {
                    var providers = VisualizationManager.visualizedContextProviders;
                    if (providers.Count > 0)
                    {
                        var ctx = providers[0].GetContext(_relevantAIGuid);
                        if (ctx != null)
                        {
                            drawer(ctx);
                        }
                    }

                    break;
                }

                case SceneVisualizationMode.AllSelectedGameObjects:
                {
                    var providers = VisualizationManager.visualizedContextProviders;
                    var count = providers.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var ctx = providers[i].GetContext(_relevantAIGuid);
                        if (ctx != null)
                        {
                            drawer(ctx);
                        }
                    }

                    break;
                }

                case SceneVisualizationMode.Custom:
                {
                    var buffer = ListBufferPool.GetBuffer<IAIContext>(4);
                    GetContextsToVisualize(buffer, _relevantAIGuid);
                    var count = buffer.Count;
                    for (int i = 0; i < count; i++)
                    {
                        drawer(buffer[i]);
                    }

                    ListBufferPool.ReturnBuffer(buffer);
                    break;
                }
            }
        }
    }
}
