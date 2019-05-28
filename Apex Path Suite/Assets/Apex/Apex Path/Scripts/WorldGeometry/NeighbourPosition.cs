/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;

    /// <summary>
    /// Each value represents a specific neighbour position in relation to a cell or grid.
    /// </summary>
    [Flags, Serializable]
    public enum NeighbourPosition
    {
        /// <summary>
        /// No position, i.e. empty mask
        /// </summary>
        None = 0,

        /// <summary>
        /// The bottom left
        /// </summary>
        BottomLeft = 1,

        /// <summary>
        /// The bottom
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// The bottom right
        /// </summary>
        BottomRight = 4,

        /// <summary>
        /// The left
        /// </summary>
        Left = 8,

        /// <summary>
        /// The cell itself
        /// </summary>
        Self = 16,

        /// <summary>
        /// The right
        /// </summary>
        Right = 32,

        /// <summary>
        /// The top left
        /// </summary>
        TopLeft = 64,

        /// <summary>
        /// The top
        /// </summary>
        Top = 128,

        /// <summary>
        /// The top right
        /// </summary>
        TopRight = 256
    }
}
