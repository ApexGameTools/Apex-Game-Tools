/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    /// <summary>
    /// Interface used by visualizers to expose the object they are visualizing.
    /// </summary>
    internal interface IVisualizedObject
    {
        /// <summary>
        /// Gets the target object of this visualizer, i.e. the visualized object.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        object target { get; }
    }
}
