/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding.MoveCost
{
    using System;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Base class for move cost providers that only work in for direction, i.e. not diagonally.
    /// </summary>
    public abstract class MoveCostBase : IMoveCost
    {
        private readonly int _cellMoveCost;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveCostBase"/> class.
        /// </summary>
        /// <param name="cellMoveCost">The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally</param>
        protected MoveCostBase(int cellMoveCost)
        {
            _cellMoveCost = cellMoveCost;
        }

        /// <summary>
        /// The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally. This is in other words the minimum cost it would take to make a move.
        /// </summary>
        /// <value>
        /// The base move cost.
        /// </value>
        public int baseMoveCost
        {
            get { return _cellMoveCost; }
        }

        /// <summary>
        /// Gets the move cost.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="other">The other node.</param>
        /// <returns>
        /// The move cost
        /// </returns>
        public virtual int GetMoveCost(IPositioned current, IPositioned other)
        {
            var dx = Math.Abs(current.position.x - other.position.x);
            var dz = Math.Abs(current.position.z - other.position.z);
            var dy = Math.Abs(current.position.y - other.position.y);

            return Mathf.RoundToInt(_cellMoveCost * (Math.Max(dx, dz) + dy));
        }

        /// <summary>
        /// Gets the heuristic.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="goal">The goal node.</param>
        /// <returns>
        /// The heuristic
        /// </returns>
        public abstract int GetHeuristic(IPositioned current, IPositioned goal);
    }
}
