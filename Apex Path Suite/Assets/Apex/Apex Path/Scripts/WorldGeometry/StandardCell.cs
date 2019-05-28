/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Represents a standard cell with shared height data.
    /// </summary>
    public class StandardCell : Cell
    {
        /// <summary>
        /// The cell factory
        /// </summary>
        public static readonly ICellFactory factory = new StandardCellFacory();

        private NeighbourPosition _heightBlockedFrom;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardCell"/> class.
        /// </summary>
        /// <param name="parent">The cell matrix that owns this cell.</param>
        /// <param name="position">The position.</param>
        /// <param name="matrixPosX">The matrix position x.</param>
        /// <param name="matrixPosZ">The matrix position z.</param>
        /// <param name="blocked">if set to <c>true</c> the cell will appear permanently blocked.</param>
        public StandardCell(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            : base(parent, position, matrixPosX, matrixPosZ, blocked)
        {
        }

        /// <summary>
        /// Gets or sets the mask of height blocked neighbours, i.e. neighbours that are not walkable from this cell to to the slope angle between them being too big.
        /// </summary>
        /// <value>
        /// The height blocked neighbours mask.
        /// </value>
        public NeighbourPosition heightBlockedFrom
        {
            get { return _heightBlockedFrom; }
            set { _heightBlockedFrom = value; }
        }

        internal NeighbourPosition heightIntializedFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether the cell is walkable from all directions.
        /// </summary>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        public override bool IsWalkableFromAllDirections(IUnitProperties unitProps)
        {
            if (!IsWalkableWithClearance(unitProps))
            {
                return false;
            }

            return (_heightBlockedFrom == NeighbourPosition.None);
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
            if (!IsWalkable(unitProps.attributes))
            {
                return false;
            }

            var pos = neighbour.GetRelativePositionTo(this);

            return (_heightBlockedFrom & pos) == 0;
        }

        /// <summary>
        /// Determines whether the cell is walkable from the specified neighbour.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        public override bool IsWalkableFromWithClearance(IGridCell neighbour, IUnitProperties unitProps)
        {
            if (!IsWalkableWithClearance(unitProps))
            {
                return false;
            }

            var pos = neighbour.GetRelativePositionTo(this);

            return (_heightBlockedFrom & pos) == 0;
        }

        private class StandardCellFacory : ICellFactory
        {
            public Cell Create(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            {
                return new StandardCell(parent, position, matrixPosX, matrixPosZ, blocked);
            }
        }
    }
}
