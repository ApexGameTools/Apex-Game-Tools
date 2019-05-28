/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Interface for cell cost strategy factories.
    /// Implement this on a MonoBehaviour class and attach it to the same GameObject as the <see cref="PathServiceComponent"/> to override the default cell cost strategy.
    /// </summary>
    public interface ICellCostStrategyFactory
    {
        /// <summary>
        /// Creates the cell cost strategy.
        /// </summary>
        /// <returns>The cell cost strategy</returns>
        ICellCostStrategy CreateCostStrategy();
    }
}
