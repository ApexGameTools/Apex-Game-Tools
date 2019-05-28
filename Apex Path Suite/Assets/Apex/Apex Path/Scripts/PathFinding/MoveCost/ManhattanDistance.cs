/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding.MoveCost
{
    using System;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Manhattan distance heuristic, i.e. only horizontal / vertical movement. Assumes distance between cells along axis are whole numbers, i.e. a uniform grid.
    /// Cell move cost constant D (_cellMoveCost) is used.
    /// </summary>
    public class ManhattanDistance : MoveCostBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManhattanDistance"/> class.
        /// </summary>
        /// <param name="cellMoveCost">The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally</param>
        public ManhattanDistance(int cellMoveCost)
            : base(cellMoveCost)
        {
        }

        /// <summary>
        /// Gets the move cost.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="other">The other node.</param>
        /// <returns>
        /// The move cost
        /// </returns>
        public override int GetMoveCost(IPositioned current, IPositioned other)
        {
            var dx = Math.Abs(current.position.x - other.position.x);
            var dz = Math.Abs(current.position.z - other.position.z);
            var dy = Math.Abs(current.position.y - other.position.y);

            return Mathf.RoundToInt(this.baseMoveCost * (dx + dz + dy));
        }

        /// <summary>
        /// Gets the heuristic.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="goal">The goal.</param>
        /// <returns>The heuristic</returns>
        public override int GetHeuristic(IPositioned current, IPositioned goal)
        {
            var dx = Math.Abs(current.position.x - goal.position.x);
            var dz = Math.Abs(current.position.z - goal.position.z);
            var dy = Math.Abs(current.position.y - goal.position.y);

            return Mathf.RoundToInt(this.baseMoveCost * (dx + dz + dy));
        }
    }
}
