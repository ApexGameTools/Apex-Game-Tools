/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.Units;

    /// <summary>
    /// Interface for grid cell basic properties.
    /// </summary>
    public interface IGridCell : IPositioned
    {
        /// <summary>
        /// Gets the parent cell matrix.
        /// </summary>
        /// <value>
        /// The parent matrix.
        /// </value>
        CellMatrix parent { get; }

        /// <summary>
        /// Gets the cell's x position in the grid matrix
        /// </summary>
        /// <value>
        /// The matrix position x.
        /// </value>
        int matrixPosX { get; }

        /// <summary>
        /// Gets the cell's z position in the grid matrix
        /// </summary>
        /// <value>
        /// The matrix position z.
        /// </value>
        int matrixPosZ { get; }

        /// <summary>
        /// Gets the arbitrary cost of walking this cell.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        int cost { get; }

        /// <summary>
        /// Gets the mask of neighbours for the cell.
        /// </summary>
        /// <value>
        /// The neighbours mask.
        /// </value>
        NeighbourPosition neighbours { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this cell is permanently blocked.
        /// Note that this is automatically set depending on level geometry when the Grid initializes, but it can also be changed manually.
        /// </summary>
        bool isPermanentlyBlocked { get; set; }

        /// <summary>
        /// Determines whether the cell is walkable. This check does not take unit size into consideration.
        /// </summary>
        /// <param name="mask">The unit's attributes.</param>
        /// <returns>
        ///   <c>true</c> if the cell is walkable, otherwise <c>false</c>
        /// </returns>
        bool IsWalkable(AttributeMask mask);

        /// <summary>
        /// Determines whether the cell is walkable to a certain unit / unit type.
        /// </summary>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        bool IsWalkableWithClearance(IUnitProperties unitProps);

        /// <summary>
        /// Determines whether the cell is walkable from all directions.
        /// </summary>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        bool IsWalkableFromAllDirections(IUnitProperties unitProps);

        /// <summary>
        /// Determines whether the cell is walkable from the specified neighbour. This check does not take unit size into consideration.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        bool IsWalkableFrom(IGridCell neighbour, IUnitProperties unitProps);

        /// <summary>
        /// Determines whether the cell is walkable from the specified neighbour.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        bool IsWalkableFromWithClearance(IGridCell neighbour, IUnitProperties unitProps);

        /// <summary>
        /// Gets this cell's relative position to the other cell.
        /// </summary>
        /// <param name="other">The other cell.</param>
        /// <returns>The relative position</returns>
        NeighbourPosition GetRelativePositionTo(IGridCell other);

        /// <summary>
        /// Gets the direction to a neighbouring cell in matrix deltas.
        /// </summary>
        /// <param name="other">The other cell.</param>
        /// <returns>A vector representing the matrix deltas to apply to reach the other cell in the matrix.</returns>
        VectorXZ GetDirectionTo(IGridCell other);

        /// <summary>
        /// Gets the neighbour at the specified matrix offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The neighbour cell or null</returns>
        Cell GetNeighbour(VectorXZ offset);

        /// <summary>
        /// Gets the neighbour (or other cell for that matter) at the specified matrix index.
        /// </summary>
        /// <param name="dx">The x offset.</param>
        /// <param name="dz">The z offset.</param>
        /// <returns>he neighbour cell or null</returns>
        Cell GetNeighbour(int dx, int dz);

        /// <summary>
        /// Gets all the neighbours of a cell.
        /// </summary>
        /// <param name="neighboursBuffer">The buffer to fill with neighbours, this must have a size of 8 (or more).</param>
        /// <returns>The number of neighbours (ranging from 3 to 8)</returns>
        int GetNeighbours(Cell[] neighboursBuffer);
    }
}
