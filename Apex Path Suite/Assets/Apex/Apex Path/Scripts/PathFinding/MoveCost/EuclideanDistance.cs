/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding.MoveCost
{
    using System;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    ///  Euclidean heuristic move cost provider
    /// </summary>
    public class EuclideanDistance : MoveCostDiagonalBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EuclideanDistance"/> class.
        /// </summary>
        /// <param name="cellMoveCost">The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally</param>
        public EuclideanDistance(int cellMoveCost)
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
            var dx = (current.position.x - other.position.x);
            var dz = (current.position.z - other.position.z);
            var dy = (current.position.y - other.position.y);

            return Mathf.RoundToInt(this.baseMoveCost * Mathf.Sqrt((dx * dx) + (dz * dz) + (dy * dy)));
        }

        /// <summary>
        /// Gets the heuristic.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="goal">The goal node.</param>
        /// <returns>
        /// The heuristic
        /// </returns>
        public override int GetHeuristic(IPositioned current, IPositioned goal)
        {
            var dx = (current.position.x - goal.position.x);
            var dz = (current.position.z - goal.position.z);
            var dy = (current.position.y - goal.position.y);

            return Mathf.RoundToInt(this.baseMoveCost * Mathf.Sqrt((dx * dx) + (dz * dz) + (dy * dy)));
        }
    }
}
