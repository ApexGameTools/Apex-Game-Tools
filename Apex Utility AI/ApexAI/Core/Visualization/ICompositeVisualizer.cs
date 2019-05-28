/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System.Collections;

    /// <summary>
    /// Interface for composite visualizers
    /// </summary>
    public interface ICompositeVisualizer
    {
        /// <summary>
        /// Gets the child items of the composite.
        /// </summary>
        IList children { get; }

        /// <summary>
        /// Adds the specified item to the composite.
        /// </summary>
        /// <param name="item">The item.</param>
        void Add(object item);
    }
}
