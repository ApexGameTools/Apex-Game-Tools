/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding.MoveCost
{
    using System;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Base class for move cost providers that include the diagonal.
    /// </summary>
    public abstract class MoveCostDiagonalBase : IMoveCost
    {
        private readonly int _cellMoveCost;
        private readonly int _cellDiagonalMoveCost;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveCostDiagonalBase"/> class.
        /// </summary>
        /// <param name="cellMoveCost">The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally</param>
        protected MoveCostDiagonalBase(int cellMoveCost)
        {
            _cellMoveCost = cellMoveCost;
            _cellDiagonalMoveCost = Mathf.FloorToInt(Consts.SquareRootTwo * cellMoveCost);
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
        /// Gets the diagonal move cost.
        /// </summary>
        /// <value>
        /// The diagonal move cost.
        /// </value>
        protected int diagonalMoveCost
        {
            get { return _cellDiagonalMoveCost; }
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

            //Its not accurate to account for the height difference by simply adding it, but it's faster and since it is the same for all it's fine.
            if (dx > 0f && dz > 0f)
            {
                return Mathf.RoundToInt((dx * _cellDiagonalMoveCost) + (dy * _cellMoveCost));
            }

            return Mathf.RoundToInt((Math.Max(dx, dz) + dy) * _cellMoveCost);
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
