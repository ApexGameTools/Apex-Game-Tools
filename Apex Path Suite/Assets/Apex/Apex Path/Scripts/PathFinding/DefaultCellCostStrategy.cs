/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.WorldGeometry;

    /// <summary>
    /// The default cost strategy, which is to just return the cell's cost.
    /// </summary>
    public class DefaultCellCostStrategy : ICellCostStrategy
    {
        /// <summary>
        /// Gets the cell cost.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="unitProperties">The unit properties.</param>
        /// <returns>
        /// The cost
        /// </returns>
        public int GetCellCost(IGridCell cell, object unitProperties)
        {
            return cell.cost;
        }
    }
}
