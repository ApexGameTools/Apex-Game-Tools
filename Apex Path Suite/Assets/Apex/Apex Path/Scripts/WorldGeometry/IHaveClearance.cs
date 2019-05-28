/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    /// <summary>
    /// Interface for cells with clearance.
    /// </summary>
    public interface IHaveClearance
    {
        /// <summary>
        /// Gets or sets the clearance value of the cell, i.e. the distance to the nearest blocked cell.
        /// </summary>
        float clearance { get; set; }
    }
}
