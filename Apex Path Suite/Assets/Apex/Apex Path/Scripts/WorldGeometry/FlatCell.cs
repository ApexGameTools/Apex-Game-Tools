/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Represents a cell with no height data
    /// </summary>
    public class FlatCell : Cell
    {
        /// <summary>
        /// The cell factory
        /// </summary>
        public static readonly ICellFactory factory = new FlatCellFacory();

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatCell"/> class.
        /// </summary>
        /// <param name="parent">The cell matrix that owns this cell.</param>
        /// <param name="position">The position.</param>
        /// <param name="matrixPosX">The matrix position x.</param>
        /// <param name="matrixPosZ">The matrix position z.</param>
        /// <param name="blocked">if set to <c>true</c> the cell will appear permanently blocked.</param>
        public FlatCell(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            : base(parent, position, matrixPosX, matrixPosZ, blocked)
        {
        }

        /// <summary>
        /// Determines whether the cell is walkable from all directions.
        /// </summary>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        public override bool IsWalkableFromAllDirections(IUnitProperties unitProps)
        {
            return IsWalkableWithClearance(unitProps);
        }

        /// <summary>
        /// Determines whether the cell is walkable from the specified neighbour. This check does not take unit size into consideration.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns>
        ///   <c>true</c> if the cell is walkable, otherwise <c>false</c>
        /// </returns>
        public override bool IsWalkableFrom(IGridCell neighbour, IUnitProperties unitProps)
        {
            return IsWalkable(unitProps.attributes);
        }

        /// <summary>
        /// Determines whether the cell is walkable from the specified neighbour.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        public override bool IsWalkableFromWithClearance(IGridCell neighbour, IUnitProperties unitProps)
        {
            return IsWalkableWithClearance(unitProps);
        }

        private class FlatCellFacory : ICellFactory
        {
            public Cell Create(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            {
                return new FlatCell(parent, position, matrixPosX, matrixPosZ, blocked);
            }
        }
    }
}
