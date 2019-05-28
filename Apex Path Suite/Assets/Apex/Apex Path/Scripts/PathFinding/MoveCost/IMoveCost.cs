/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding.MoveCost
{
    using Apex.WorldGeometry;

    /// <summary>
    /// Interface for move cost providers.
    /// </summary>
    public interface IMoveCost
    {
        /// <summary>
        /// The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally. This is in other words the minimum cost it would take to make a move.
        /// </summary>
        /// <value>
        /// The base move cost.
        /// </value>
        int baseMoveCost { get; }

        /// <summary>
        /// Gets the move cost.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="other">The other node.</param>
        /// <returns>The move cost</returns>
        int GetMoveCost(IPositioned current, IPositioned other);

        /// <summary>
        /// Gets the heuristic.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="goal">The goal node.</param>
        /// <returns>The heuristic</returns>
        int GetHeuristic(IPositioned current, IPositioned goal);
    }
}
