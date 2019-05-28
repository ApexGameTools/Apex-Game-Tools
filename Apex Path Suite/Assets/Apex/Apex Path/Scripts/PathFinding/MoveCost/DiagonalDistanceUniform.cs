/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding.MoveCost
{
    using System;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Diagonal distance heuristic. Assumes distance between cells along axis are whole numbers, i.e. a uniform grid. And also treats diagonals as the same cost as horizontal, vertical movement.
    /// Cell move cost constant D (_cellMoveCost) is used.
    /// </summary>
    public class DiagonalDistanceUniform : MoveCostBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagonalDistanceUniform"/> class.
        /// </summary>
        /// <param name="cellMoveCost">The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally</param>
        public DiagonalDistanceUniform(int cellMoveCost)
            : base(cellMoveCost)
        {
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
            
            return Mathf.RoundToInt(this.baseMoveCost * (Math.Max(dx, dz) + dy));
        }
    }
}
