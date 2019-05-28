/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    /// <summary>
    /// Represents which objects are visualized by scene visualizers, i.e. derivatives of <see cref="ContextGizmoVisualizerComponent"/>
    /// </summary>
    public enum SceneVisualizationMode
    {
        /// <summary>
        /// Visualization is done for the single selected game object. If more are selected the first one is the only one visualized.
        /// </summary>
        SingleSelectedGameObject,

        /// <summary>
        /// Visualization is done for all selected game objects.
        /// </summary>
        AllSelectedGameObjects,

        /// <summary>
        /// Visualization is controlled by a custom implementation on the visualizer, e.g. <see cref="ContextVisualizerComponent.GetContextsToVisualize"/>
        /// </summary>
        Custom
    }
}
