/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.WorldGeometry;

    /// <summary>
    /// Interface for cost strategies.
    /// </summary>
    public interface ICellCostStrategy
    {
        /// <summary>
        /// Gets the cell cost.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="unitProperties">The unit properties.</param>
        /// <returns>The cost</returns>
        int GetCellCost(IGridCell cell, object unitProperties);

        //void ApplyCellCost(IGridCell cell, int cost);

        //void RemoveCellCost(IGridCell cell, int cost);
    }
}
