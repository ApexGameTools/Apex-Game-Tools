/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections.Generic;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// The grid interface
    /// </summary>
    public interface IGrid
    {
        /// <summary>
        /// Gets the origin of the grid, i.e. its center position.
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        Vector3 origin { get; }

        /// <summary>
        /// Gets the grid size along the x-axis.
        /// </summary>
        /// <value>
        /// The x size.
        /// </value>
        int sizeX { get; }

        /// <summary>
        /// Gets the grid size along the z-axis.
        /// </summary>
        /// <value>
        /// The z size.
        /// </value>
        int sizeZ { get; }

        /// <summary>
        /// Gets the grid cell size.
        /// </summary>
        /// <value>
        /// The cell size.
        /// </value>
        float cellSize { get; }

        /// <summary>
        /// Gets the bounds of the grid plane.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        Bounds bounds { get; }

        /// <summary>
        /// Gets the left perimeter.
        /// </summary>
        /// <value>
        /// The left perimeter.
        /// </value>
        Perimeter left { get; }

        /// <summary>
        /// Gets the right perimeter.
        /// </summary>
        /// <value>
        /// The right perimeter.
        /// </value>
        Perimeter right { get; }

        /// <summary>
        /// Gets the top perimeter.
        /// </summary>
        /// <value>
        /// The top perimeter.
        /// </value>
        Perimeter top { get; }

        /// <summary>
        /// Gets the bottom perimeter.
        /// </summary>
        /// <value>
        /// The bottom perimeter.
        /// </value>
        Perimeter bottom { get; }

        /// <summary>
        /// Gets the cells of the grid.
        /// </summary>
        /// <value>
        /// The cells.
        /// </value>
        IEnumerable<Cell> cells { get; }

        /// <summary>
        /// Gets the cell matrix.
        /// </summary>
        /// <value>
        /// The cell matrix.
        /// </value>
        CellMatrix cellMatrix { get; }

        /// <summary>
        /// Gets the grid sections.
        /// </summary>
        /// <value>
        /// The grid sections.
        /// </value>
        GridSection[] gridSections { get; }

        /// <summary>
        /// Determines whether the bounds of the grid contains the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns><c>true</c> if the position is contained; otherwise false.</returns>
        bool Contains(Vector3 pos);

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The cell at the position or null if no cell is at that position (i.e. outside the grid)</returns>
        Cell GetCell(Vector3 position);

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="adjustToBounds">if set to <c>true</c> the cell closest to the specified position will be returned, ie.e. if the position is outside the grid bounds.</param>
        /// <returns>
        /// The cell at the position or either null or the closest cell if no cell is at that exact position (i.e. outside the grid), depending on the <paramref name="adjustToBounds"/> argument.
        /// </returns>
        Cell GetCell(Vector3 position, bool adjustToBounds);

        /// <summary>
        /// Gets the nearest walkable cell on the specified perimeter.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="perimeter">The perimeter to check.</param>
        /// <param name="unitProps">The requester units properties.</param>
        /// <param name="adjustPositionToPerimiter">if set to <c>true</c> <paramref name="position"/> will be adjusted to be on the perimeter (if it is not already).</param>
        /// <returns>The nearest walkable perimeter cell</returns>
        Cell GetNearestWalkablePerimeterCell(Vector3 position, Vector3 perimeter, IUnitProperties unitProps, bool adjustPositionToPerimiter);

        /// <summary>
        /// Gets the neighbour of the specified cell.
        /// </summary>
        /// <param name="cell">The reference cell.</param>
        /// <param name="dx">The delta x position in the matrix.</param>
        /// <param name="dz">The delta z position in the matrix.</param>
        /// <returns>The neighbour or null if none is found.</returns>
        Cell GetNeighbour(IGridCell cell, int dx, int dz);

        /// <summary>
        /// Tries the get a walkable neighbour.
        /// </summary>
        /// <param name="cell">The reference cell.</param>
        /// <param name="dx">The delta x position in the matrix.</param>
        /// <param name="dz">The delta z position in the matrix.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <param name="neighbour">The neighbour or null if none is found.</param>
        /// <returns><c>true</c> if a walkable neighbour is found, in which case <paramref name="neighbour"/> will have the reference to that neighbour. Otherwise <c>false</c>.</returns>
        bool TryGetWalkableNeighbour(IGridCell cell, int dx, int dz, IUnitProperties unitProps, out Cell neighbour);

        /// <summary>
        /// Gets the cell range from a position and a radius.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>All cells within <paramref name="radius"/> of <paramref name="position"/></returns>
        IEnumerable<Cell> GetCellRange(Vector3 position, float radius);

        /// <summary>
        /// Gets the cell range from a position and a radius.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="result">The result list that will be populated with all cells within <paramref name="radius"/> of <paramref name="position"/>.</param>
        void GetCellRange(Vector3 position, float radius, ICollection<Cell> result);

        /// <summary>
        /// Gets the cells covered by the specified bounding rectangle.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <returns>All cells covered by the specified bounding rectangle.</returns>
        IEnumerable<Cell> GetCoveredCells(Bounds bounds);

        /// <summary>
        /// Gets the cells covered by the specified bounding rectangle.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="result">The result list that will be populated with all cells covered by the specified bounding rectangle.</param>
        void GetCoveredCells(Bounds bounds, ICollection<Cell> result);

        /// <summary>
        /// Applies an action to a range of cells given by a position and a radius.
        /// This can be used to modify cell properties or collect data from cells.
        /// </summary>
        /// <param name="position">The reference position (center).</param>
        /// <param name="radius">The radius.</param>
        /// <param name="act">The action to apply. This can be anything from modifying the items to collecting data from the items.</param>
        void Apply(Vector3 position, float radius, Action<Cell> act);

        /// <summary>
        /// Applies an action to a range of cells.
        /// This can be used to modify cell properties or collect data from cells.
        /// </summary>
        /// <param name="bounds">The bounds encapsulating the cells.</param>
        /// <param name="act">The action to apply. This can be anything from modifying the items to collecting data from the items.</param>
        void Apply(Bounds bounds, Action<Cell> act);

        /// <summary>
        /// Gets the sections that intersect the specified bounding rectangle.
        /// </summary>
        /// <param name="b">The bounding rectangle.</param>
        /// <param name="result">The result list that will be populated with the sections that match.</param>
        void GetSections(Bounds b, ICollection<GridSection> result);

        /// <summary>
        /// Touches the sections at the specified position, marking them as changed.
        /// </summary>
        /// <param name="position">The position.</param>
        void TouchSections(Vector3 position);

        /// <summary>
        /// Touches the sections overlapping with the specified bounds, marking them as changed.
        /// </summary>
        /// <param name="b">The bounds.</param>
        void TouchSections(Bounds b);

        /// <summary>
        /// Touches the sections overlapping with the specified bounds, marking them as changed.
        /// </summary>
        /// <param name="b">The bounds.</param>
        void TouchSections(MatrixBounds b);

        /// <summary>
        /// Determines whether one or more sections that contain the specified position have changed.
        /// </summary>
        /// <param name="pos">The reference position.</param>
        /// <param name="time">The time to check against.</param>
        /// <returns><c>true</c> if one or more sections have changed since <paramref name="time"/></returns>
        bool HasSectionsChangedSince(Vector3 pos, float time);

        /// <summary>
        /// Gets a layer of cells around a center. A layer is defined as the outer cells of the concentric square given by the cellDistance argument.
        /// Distance of 0 is the cell itself, Distance of 1 is the 8 neighbouring cells, Distance of 2 is the 16  outer most neighboring cells to layer 1, etc. Think onion.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="cellDistance">The cell distance, 0 being the cell itself.</param>
        /// <returns>The list of cells making up the layer.</returns>
        IEnumerable<Cell> GetConcentricNeighbours(IGridCell center, int cellDistance);

        /// <summary>
        /// Gets a layer of cells around a center. A layer is defined as the outer cells of the concentric square given by the cellDistance argument.
        /// Distance of 0 is the cell itself, Distance of 1 is the 8 neighbouring cells, Distance of 2 is the 16  outer most neighboring cells to layer 1, etc. Think onion.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="cellDistance">The cell distance, 0 being the cell itself.</param>
        /// <param name="array">A dynamic array to populate with the found neighbour cells</param>
        void GetConcentricNeighbours(IGridCell center, int cellDistance, DynamicArray<Cell> array);

        /// <summary>
        /// Gets a walkable neighbours iterator. Optimization used by the pathing engines.
        /// </summary>
        /// <param name="c">The reference cell.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <param name="excludeCornerCutting">whether to exclude corner cutting.</param>
        /// <returns>An enumerator of the walkable cells</returns>
        IEnumerator<IPathNode> GetWalkableNeighboursIterator(IGridCell c, IUnitProperties unitProps, bool excludeCornerCutting);

        /// <summary>
        /// Updates the specified region of the grid with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        void Update(Bounds extent, int maxMillisecondsUsedPerFrame);

        /// <summary>
        /// Updates the specified region of the grid with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        void Update(Bounds extent, int maxMillisecondsUsedPerFrame, Action callback);

        /// <summary>
        /// Updates the specified region of the grid with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="blockWhileUpdating">If <c>true</c> the cells about to be updated will be marked as blocked while the update is in progress.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        void Update(Bounds extent, int maxMillisecondsUsedPerFrame, bool blockWhileUpdating, Action callback);
    }
}
