/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.PathFinding.MoveCost;

    /// <summary>
    /// Interface for move cost provider factories.
    /// Implement this on a MonoBehaviour class and attach it to the same GameObject as the <see cref="PathServiceComponent"/> to override the default move cost provider.
    /// </summary>
    public interface IMoveCostFactory
    {
        /// <summary>
        /// Creates the move cost provider.
        /// </summary>
        /// <returns>The move cost provider</returns>
        IMoveCost CreateMoveCostProvider();
    }
}
