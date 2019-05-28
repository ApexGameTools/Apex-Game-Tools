/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Represent different types of move cost providers
    /// </summary>
    public enum CostProviderType
    {
        /// <summary>
        /// Diagonal distance <see cref="MoveCost.DiagonalDistance"/>
        /// </summary>
        Diagonal,

        /// <summary>
        /// Euclidean distance <see cref="MoveCost.EuclideanDistance"/>
        /// </summary>
        Euclidean,

        /// <summary>
        /// Cardinal distance <see cref="MoveCost.CardinalDistance"/>
        /// </summary>
        Cardinal,

        /// <summary>
        /// Manhattan distance <see cref="MoveCost.ManhattanDistance"/>
        /// </summary>
        Manhattan,

        /// <summary>
        /// Custom distance defined by factory <see cref="IMoveCostFactory"/>
        /// </summary>
        Custom
    }
}
