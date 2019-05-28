/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    /// <summary>
    /// Interface for entities that have a grid associated with them.
    /// </summary>
    public interface IGridSource
    {
        /// <summary>
        /// Gets the grid.
        /// </summary>
        /// <value>
        /// The grid.
        /// </value>
        IGrid grid { get; }
    }
}
