/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.PathFinding;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// A grid that divides the game world or parts of the game world into equally sized <see cref="Cell"/>s.
    /// </summary>
    public sealed class Grid : IGrid
    {
        private CellMatrix _cellMatrix;
        private GridSection[] _gridSections;
        private Vector3 _origin;
        private int _sizeX;
        private int _sizeZ;
        private float _cellSize;
        private Perimeter _left;
        private Perimeter _right;
        private Perimeter _top;
        private Perimeter _bottom;
        private WalkableNeighboursEnumerator _walkableIter;

        private Grid()
        {
            _walkableIter = new WalkableNeighboursEnumerator(this);
        }

        /// <summary>
        /// Gets the origin of the grid, i.e. its center position.
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        public Vector3 origin
        {
            get { return _origin; }
        }

        /// <summary>
        /// Gets the grid size along the x-axis.
        /// </summary>
        /// <value>
        /// The x size.
        /// </value>
        public int sizeX
        {
            get { return _sizeX; }
        }

        /// <summary>
        /// Gets the grid size along the z-axis.
        /// </summary>
        /// <value>
        /// The z size.
        /// </value>
        public int sizeZ
        {
            get { return _sizeZ; }
        }

        /// <summary>
        /// Gets the grid cell size.
        /// </summary>
        /// <value>
        /// The cell size.
        /// </value>
        public float cellSize
        {
            get { return _cellSize; }
        }

        /// <summary>
        /// Gets the bounds of the grid.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        public Bounds bounds
        {
            get { return _cellMatrix.bounds; }
        }

        /// <summary>
        /// Gets the left perimeter.
        /// </summary>
        /// <value>
        /// The left perimeter.
        /// </value>
        public Perimeter left
        {
            get { return _left; }
        }

        /// <summary>
        /// Gets the right perimeter.
        /// </summary>
        /// <value>
        /// The right perimeter.
        /// </value>
        public Perimeter right
        {
            get { return _right; }
        }

        /// <summary>
        /// Gets the top perimeter.
        /// </summary>
        /// <value>
        /// The top perimeter.
        /// </value>
        public Perimeter top
        {
            get { return _top; }
        }

        /// <summary>
        /// Gets the bottom perimeter.
        /// </summary>
        /// <value>
        /// The bottom perimeter.
        /// </value>
        public Perimeter bottom
        {
            get { return _bottom; }
        }

        /// <summary>
        /// Gets the cells of the grid.
        /// </summary>
        /// <value>
        /// The cells.
        /// </value>
        public IEnumerable<Cell> cells
        {
            get
            {
                return _cellMatrix.items;
            }
        }

        /// <summary>
        /// Gets the cell matrix.
        /// </summary>
        /// <value>
        /// The cell matrix.
        /// </value>
        public CellMatrix cellMatrix
        {
            get { return _cellMatrix; }
        }

        /// <summary>
        /// Gets the grid sections.
        /// </summary>
        /// <value>
        /// The grid sections.
        /// </value>
        public GridSection[] gridSections
        {
            get { return _gridSections; }
        }

        /// <summary>
        /// Creates a grid.
        /// </summary>
        /// <param name="matrix">The cell matrix.</param>
        /// <param name="gridSections">The grid sections array</param>
        /// <returns>The grid</returns>
        public static Grid Create(CellMatrix matrix, GridSection[] gridSections)
        {
            var g = new Grid();

            g.Initialize(matrix, gridSections);

            return g;
        }

        /// <summary>
        /// Determines whether the bounds of the grid contains the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        ///   <c>true</c> if the position is contained; otherwise false.
        /// </returns>
        public bool Contains(Vector3 pos)
        {
            return _cellMatrix.Contains(pos);
        }

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>
        /// The cell at the position or null if no cell is at that position (i.e. outside the grid)
        /// </returns>
        public Cell GetCell(Vector3 position)
        {
            return _cellMatrix.GetCell(position, false);
        }

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="adjustToBounds">if set to <c>true</c> the cell closest to the specified position will be returned, ie.e. if the position is outside the grid bounds.</param>
        /// <returns>
        /// The cell at the position or either null or the closest cell if no cell is at that exact position (i.e. outside the grid), depending on the <paramref name="adjustToBounds"/> argument.
        /// </returns>
        public Cell GetCell(Vector3 position, bool adjustToBounds)
        {
            return _cellMatrix.GetCell(position, adjustToBounds);
        }

        /// <summary>
        /// Gets cells within the specified radius of a position. Only cells whose centers (the cells position) are within the radius are included.
        /// </summary>
        /// <param name="position">The center position</param>
        /// <param name="radius">The radius.</param>
        /// <returns>The list of cells whose centers are within the radius</returns>
        public IEnumerable<Cell> GetCellRange(Vector3 position, float radius)
        {
            return GetCellRange(position, radius, radius, true);
        }

        /// <summary>
        /// Gets the cell range from a position and a radius.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="result">The result list that will be populated with all cells within <paramref name="radius" /> of <paramref name="position" />.</param>
        public void GetCellRange(Vector3 position, float radius, ICollection<Cell> result)
        {
            GetCellRange(position, radius, radius, true, result);
        }

        /// <summary>
        /// Gets the sections that intersect the specified bounding rectangle.
        /// </summary>
        /// <param name="b">The bounding rectangle.</param>
        /// <param name="result">The result list that will be populated with the sections that match.</param>
        public void GetSections(Bounds b, ICollection<GridSection> result)
        {
            for (int i = 0; i < _gridSections.Length; i++)
            {
                if (_gridSections[i].bounds.Overlaps(b))
                {
                    result.Add(_gridSections[i]);
                }
            }
        }

        /// <summary>
        /// Touches the sections at the specified position, marking them as changed.
        /// </summary>
        /// <param name="position">The position.</param>
        public void TouchSections(Vector3 position)
        {
            for (int i = 0; i < _gridSections.Length; i++)
            {
                if (_gridSections[i].bounds.Contains(position))
                {
                    _gridSections[i].Touch();
                }
            }
        }

        /// <summary>
        /// Touches the sections overlapping with the specified bounds, marking them as changed.
        /// </summary>
        /// <param name="b">The bounds.</param>
        public void TouchSections(Bounds b)
        {
            for (int i = 0; i < _gridSections.Length; i++)
            {
                if (_gridSections[i].bounds.Overlaps(b))
                {
                    _gridSections[i].Touch();
                }
            }
        }

        /// <summary>
        /// Touches the sections overlapping with the specified bounds, marking them as changed.
        /// </summary>
        /// <param name="b">The bounds.</param>
        public void TouchSections(MatrixBounds b)
        {
            var rect = new RectangleXZ(
                        _cellMatrix.start.x + (b.minColumn * _cellSize),
                        _cellMatrix.start.z + (b.minRow * _cellSize),
                        b.maxColumn * _cellSize,
                        b.minRow * _cellSize);

            for (int i = 0; i < _gridSections.Length; i++)
            {
                if (_gridSections[i].bounds.Overlaps(rect))
                {
                    _gridSections[i].Touch();
                }
            }
        }

        /// <summary>
        /// Determines whether one or more sections that contain the specified position have changed.
        /// </summary>
        /// <param name="pos">The reference position.</param>
        /// <param name="time">The time to check against.</param>
        /// <returns>
        ///   <c>true</c> if one or more sections have changed since <paramref name="time" />
        /// </returns>
        public bool HasSectionsChangedSince(Vector3 pos, float time)
        {
            pos = pos.AdjustAxis(_origin.y, Axis.Y);
            for (int i = 0; i < _gridSections.Length; i++)
            {
                var section = _gridSections[i];
                if (section.bounds.Contains(pos) && section.lastChanged >= time)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the nearest walkable cell on the specified perimeter.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="perimeter">The perimeter to check.</param>
        /// <param name="unitProps">The requester units properties.</param>
        /// <param name="adjustPositionToPerimiter">if set to <c>true</c> <paramref name="position" /> will be adjusted to be on the perimeter (if it is not already).</param>
        /// <returns>The nearest walkable perimeter cell</returns>
        public Cell GetNearestWalkablePerimeterCell(Vector3 position, Vector3 perimeter, IUnitProperties unitProps, bool adjustPositionToPerimiter)
        {
            //Adjust the cell to the perimeter and within bounds
            if (adjustPositionToPerimiter)
            {
                if (perimeter.x != 0)
                {
                    position.x = GetPerimeterEdge(perimeter, true);
                    var dzb = this.bottom.edgeMid;
                    var dzf = this.top.edgeMid;
                    if (position.z < dzb)
                    {
                        position.z = dzb;
                    }
                    else if (position.z > dzf)
                    {
                        position.z = dzf;
                    }
                }
                else if (perimeter.z != 0)
                {
                    position.z = GetPerimeterEdge(perimeter, true);
                    var dxl = this.left.edgeMid;
                    var dxr = this.right.edgeMid;
                    if (position.x < dxl)
                    {
                        position.x = dxl;
                    }
                    else if (position.x > dxr)
                    {
                        position.x = dxr;
                    }
                }
            }

            //If this is already the nearest walkable cell return it
            var c = GetCell(position);
            if (c == null)
            {
                return null;
            }

            if (c.IsWalkableWithClearance(unitProps))
            {
                return c;
            }

            //Traverse along the perimeter to find the closet walkable cell
            if (perimeter.x != 0)
            {
                return TraversePerimeterX(c, 1, unitProps);
            }

            return TraversePerimeterZ(c, 1, unitProps);
        }

        /// <summary>
        /// Gets the cells covered by the given bounds, even cells barely touched by the bounds are included.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <returns>The list of cells that are in any way covered by the bounds.</returns>
        public IEnumerable<Cell> GetCoveredCells(Bounds bounds)
        {
            return GetCellRange(bounds.center, bounds.extents.x, bounds.extents.z, false);
        }

        /// <summary>
        /// Gets the cells covered by the specified bounding rectangle.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="result">The result list that will be populated with all cells covered by the specified bounding rectangle.</param>
        public void GetCoveredCells(Bounds bounds, ICollection<Cell> result)
        {
            GetCellRange(bounds.center, bounds.extents.x, bounds.extents.z, false, result);
        }

        /// <summary>
        /// Applies an action to a range of cells given by a position and a radius.
        /// This can be used to modify cell properties or collect data from cells.
        /// </summary>
        /// <param name="position">The reference position (center).</param>
        /// <param name="radius">The radius.</param>
        /// <param name="act">The action to apply. This can be anything from modifying the items to collecting data from the items.</param>
        public void Apply(Vector3 position, float radius, Action<Cell> act)
        {
            Apply(position, radius, radius, act);
        }

        /// <summary>
        /// Applies an action to a range of cells.
        /// This can be used to modify cell properties or collect data from cells.
        /// </summary>
        /// <param name="bounds">The bounds encapsulating the cells.</param>
        /// <param name="act">The action to apply. This can be anything from modifying the items to collecting data from the items.</param>
        public void Apply(Bounds bounds, Action<Cell> act)
        {
            Apply(bounds.center, bounds.extents.x, bounds.extents.z, act);
        }

        /// <summary>
        /// Gets a layer of cells around a center. A layer is defined as the outer cells of the concentric square given by the cellDistance argument.
        /// Distance of 0 is the cell itself, Distance of 1 is the 8 neighbouring cells, Distance of 2 is the 16  outer most neighboring cells to layer 1, etc. Think onion.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="cellDistance">The cell distance, 0 being the cell itself.</param>
        /// <returns>The list of cells making up the layer.</returns>
        public IEnumerable<Cell> GetConcentricNeighbours(IGridCell center, int cellDistance)
        {
            return _cellMatrix.GetConcentricNeighbours(center.matrixPosX, center.matrixPosZ, cellDistance);
        }

        /// <summary>
        /// Gets a layer of cells around a center. A layer is defined as the outer cells of the concentric square given by the cellDistance argument.
        /// Distance of 0 is the cell itself, Distance of 1 is the 8 neighbouring cells, Distance of 2 is the 16  outer most neighboring cells to layer 1, etc. Think onion.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="cellDistance">The cell distance, 0 being the cell itself.</param>
        /// <param name="array">A dynamic array to populate with the found neighbour cells</param>
        public void GetConcentricNeighbours(IGridCell center, int cellDistance, DynamicArray<Cell> array)
        {
            _cellMatrix.GetConcentricNeighbours(center.matrixPosX, center.matrixPosZ, cellDistance, array);
        }

        /// <summary>
        /// Gets the neighbour of the specified cell.
        /// </summary>
        /// <param name="cell">The reference cell.</param>
        /// <param name="dx">The delta x position in the matrix.</param>
        /// <param name="dz">The delta z position in the matrix.</param>
        /// <returns>
        /// The neighbour or null if none is found.
        /// </returns>
        public Cell GetNeighbour(IGridCell cell, int dx, int dz)
        {
            var x = cell.matrixPosX + dx;
            var z = cell.matrixPosZ + dz;

            return _cellMatrix[x, z];
        }

        /// <summary>
        /// Tries the get a walkable neighbour.
        /// </summary>
        /// <param name="cell">The reference cell.</param>
        /// <param name="dx">The delta x position in the matrix.</param>
        /// <param name="dz">The delta z position in the matrix.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <param name="neighbour">The neighbour or null if none is found.</param>
        /// <returns>
        ///   <c>true</c> if a walkable neighbour is found, in which case <paramref name="neighbour" /> will have the reference to that neighbour. Otherwise <c>false</c>.
        /// </returns>
        public bool TryGetWalkableNeighbour(IGridCell cell, int dx, int dz, IUnitProperties unitProps, out Cell neighbour)
        {
            var x = cell.matrixPosX + dx;
            var z = cell.matrixPosZ + dz;

            neighbour = _cellMatrix[x, z];

            if (neighbour == null)
            {
                return false;
            }

            return neighbour.IsWalkableFromWithClearance(cell, unitProps);
        }

        /// <summary>
        /// Gets a walkable neighbours iterator. Optimization used by the pathing engines.
        /// </summary>
        /// <param name="c">The reference cell.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <param name="excludeCornerCutting">whether to exclude corner cutting.</param>
        /// <returns>
        /// An enumerator of the walkable cells
        /// </returns>
        public IEnumerator<IPathNode> GetWalkableNeighboursIterator(IGridCell c, IUnitProperties unitProps, bool excludeCornerCutting)
        {
            _walkableIter.Prepare(c, unitProps, excludeCornerCutting);
            return _walkableIter;
        }

        /// <summary>
        /// Updates the specified region of the grid with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        public void Update(Bounds extent, int maxMillisecondsUsedPerFrame)
        {
            Update(extent, maxMillisecondsUsedPerFrame, false, null);
        }

        /// <summary>
        /// Updates the specified region of the grid with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        public void Update(Bounds extent, int maxMillisecondsUsedPerFrame, Action callback)
        {
            Update(extent, maxMillisecondsUsedPerFrame, false, callback);
        }

        /// <summary>
        /// Updates the specified region of the grid with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="blockWhileUpdating">If <c>true</c> the cells about to be updated will be marked as blocked while the update is in progress.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        public void Update(Bounds extent, int maxMillisecondsUsedPerFrame, bool blockWhileUpdating, Action callback)
        {
            LoadBalancer.defaultBalancer.Add(
                new LongRunningAction(() => this.UpdateInternal(extent, blockWhileUpdating), maxMillisecondsUsedPerFrame, callback),
                0.01f,
                true);
        }

        private IEnumerator UpdateInternal(Bounds extent, bool blockWhileUpdating)
        {
            ManualDynamicObstacle mo = null;
            if (blockWhileUpdating)
            {
                mo = new ManualDynamicObstacle(extent);
                mo.Toggle(true);
            }

            //Update the matrix
            var iter = _cellMatrix.Update(extent);
            if (iter != null)
            {
                while (iter.MoveNext())
                {
                    yield return null;
                }
            }

            if (blockWhileUpdating)
            {
                mo.Toggle(false);
            }
            else
            {
                //Get the sections involved and touch them
                for (int i = 0; i < _gridSections.Length; i++)
                {
                    if (_gridSections[i].bounds.Overlaps(extent))
                    {
                        _gridSections[i].Touch();
                    }
                }
            }
        }

        private void Initialize(CellMatrix matrix, GridSection[] gridSections)
        {
            //Setup the basic state
            _cellMatrix = matrix;
            _gridSections = gridSections;
            _origin = _cellMatrix.origin;
            _cellSize = _cellMatrix.cellSize;
            _sizeX = _cellMatrix.columns;
            _sizeZ = _cellMatrix.rows;

            //Perimeters
            _left = new Perimeter(Vector3.left, GetPerimeterEdge(Vector3.left, false), GetPerimeterEdge(Vector3.left, true));
            _right = new Perimeter(Vector3.right, GetPerimeterEdge(Vector3.right, false), GetPerimeterEdge(Vector3.right, true));
            _top = new Perimeter(Vector3.forward, GetPerimeterEdge(Vector3.forward, false), GetPerimeterEdge(Vector3.forward, true));
            _bottom = new Perimeter(Vector3.back, GetPerimeterEdge(Vector3.back, false), GetPerimeterEdge(Vector3.back, true));

            _left.SetPerpendiculars(_top, _bottom, _cellSize);
            _right.SetPerpendiculars(_bottom, _top, _cellSize);
            _top.SetPerpendiculars(_right, _left, _cellSize);
            _bottom.SetPerpendiculars(_left, _right, _cellSize);
        }

        private IEnumerable<Cell> GetCellRange(Vector3 position, float radiusX, float radiusZ, bool requireCenterCell)
        {
            float requiredOverlap = 0.0f;
            if (requireCenterCell)
            {
                requiredOverlap = _cellSize * 0.5f;
            }

            //We don't adjust to matrix bounds since that is also done in the call to GetRange
            var b = _cellMatrix.GetMatrixBounds(position, radiusX, radiusZ, requiredOverlap, false);
            return _cellMatrix.GetRange(b);
        }

        private void GetCellRange(Vector3 position, float radiusX, float radiusZ, bool requireCenterCell, ICollection<Cell> result)
        {
            float requiredOverlap = 0.0f;
            if (requireCenterCell)
            {
                requiredOverlap = _cellSize * 0.5f;
            }

            //We don't adjust to matrix bounds since that is also done in the call to GetRange
            var b = _cellMatrix.GetMatrixBounds(position, radiusX, radiusZ, requiredOverlap, false);
            _cellMatrix.GetRange(b, result);
        }

        private void Apply(Vector3 position, float radiusX, float radiusZ, Action<Cell> act)
        {
            float requiredOverlap = _cellSize * 0.5f;

            //We don't adjust to matrix bounds since that is also done in the call to GetRange
            var b = _cellMatrix.GetMatrixBounds(position, radiusX, radiusZ, requiredOverlap, false);
            _cellMatrix.Apply(b, act);
        }

        /// <summary>
        /// Gets the fixed coordinate of the perimeter row/column of cells, as defined by the perimeter argument.
        /// </summary>
        /// <param name="perimeter">Must be a vector pointing in either -x, x, -z or z direction with a length of one.</param>
        /// <param name="mid">Whether to get the middle of the perimeter edge or the actual edge.</param>
        /// <returns>The edge coord</returns>
        private float GetPerimeterEdge(Vector3 perimeter, bool mid)
        {
            int adj = mid ? 1 : 0;

            if (perimeter.x != 0)
            {
                return _origin.x + (perimeter.x * (_sizeX - adj) * (_cellSize / 2.0f));
            }

            if (perimeter.z != 0)
            {
                return _origin.z + (perimeter.z * (_sizeZ - adj) * (_cellSize / 2.0f));
            }

            return 0.0f;
        }

        private Cell TraversePerimeterX(Cell c, int step, IUnitProperties unitProps)
        {
            var up = c.matrixPosZ + step;
            bool lookUp = (up < _sizeZ);
            if (lookUp)
            {
                var cup = _cellMatrix.rawMatrix[c.matrixPosX, up];
                if (cup.IsWalkableWithClearance(unitProps))
                {
                    return cup;
                }
            }

            var down = c.matrixPosZ - step;
            bool lookDown = (down >= 0);
            if (lookDown)
            {
                var cdown = _cellMatrix.rawMatrix[c.matrixPosX, down];
                if (cdown.IsWalkableWithClearance(unitProps))
                {
                    return cdown;
                }
            }

            if (lookUp || lookDown)
            {
                return TraversePerimeterX(c, step + 1, unitProps);
            }

            return null;
        }

        private Cell TraversePerimeterZ(Cell c, int step, IUnitProperties unitProps)
        {
            var right = c.matrixPosX + step;
            bool lookRight = (right < _sizeX);
            if (lookRight)
            {
                var cr = _cellMatrix.rawMatrix[right, c.matrixPosZ];
                if (cr.IsWalkableWithClearance(unitProps))
                {
                    return cr;
                }
            }

            var left = c.matrixPosX - step;
            bool lookLeft = (left >= 0);
            if (lookLeft)
            {
                var cl = _cellMatrix.rawMatrix[left, c.matrixPosZ];
                if (cl.IsWalkableWithClearance(unitProps))
                {
                    return cl;
                }
            }

            if (lookRight || lookLeft)
            {
                return TraversePerimeterZ(c, step + 1, unitProps);
            }

            return null;
        }

        [Serializable]
        private class WalkableNeighboursEnumerator : IEnumerator<IPathNode>
        {
            private readonly Grid _g;
            private readonly IPathNode[] _neighbours;

            private int _index;
            private IPathNode _current;

            internal WalkableNeighboursEnumerator(Grid g)
            {
                _g = g;
                _current = null;
                _neighbours = new IPathNode[8];
            }

            public IPathNode Current
            {
                get { return _current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_index == 8)
                {
                    _current = null;
                    return false;
                }

                _current = _neighbours[_index++];
                if (_current == null)
                {
                    return false;
                }

                return true;
            }

            public void Reset()
            {
                Array.Clear(_neighbours, 0, 8);
                _current = null;
                _index = 0;
            }

            internal void Prepare(IGridCell c, IUnitProperties unitProps, bool excludeCornerCutting)
            {
                Reset();

                int idx = 0;

                Cell n;

                //Straight move neighbours
                bool uw = _g.TryGetWalkableNeighbour(c, 0, 1, unitProps, out n);
                if (uw)
                {
                    _neighbours[idx++] = n;
                }

                bool dw = _g.TryGetWalkableNeighbour(c, 0, -1, unitProps, out n);
                if (dw)
                {
                    _neighbours[idx++] = n;
                }

                bool rw = _g.TryGetWalkableNeighbour(c, 1, 0, unitProps, out n);
                if (rw)
                {
                    _neighbours[idx++] = n;
                }

                bool lw = _g.TryGetWalkableNeighbour(c, -1, 0, unitProps, out n);
                if (lw)
                {
                    _neighbours[idx++] = n;
                }

                //Diagonal neighbours. First determine if they are unwalkable as a consequence of their straight neighbours
                bool urw, drw, dlw, ulw;
                if (excludeCornerCutting)
                {
                    urw = uw && rw;
                    drw = dw && rw;
                    dlw = dw && lw;
                    ulw = uw && lw;
                }
                else
                {
                    urw = uw || rw;
                    drw = dw || rw;
                    dlw = dw || lw;
                    ulw = uw || lw;
                }

                urw = urw && _g.TryGetWalkableNeighbour(c, 1, 1, unitProps, out n);
                if (urw)
                {
                    _neighbours[idx++] = n;
                }

                drw = drw && _g.TryGetWalkableNeighbour(c, 1, -1, unitProps, out n);
                if (drw)
                {
                    _neighbours[idx++] = n;
                }

                dlw = dlw && _g.TryGetWalkableNeighbour(c, -1, -1, unitProps, out n);
                if (dlw)
                {
                    _neighbours[idx++] = n;
                }

                ulw = ulw && _g.TryGetWalkableNeighbour(c, -1, 1, unitProps, out n);
                if (ulw)
                {
                    _neighbours[idx++] = n;
                }
            }
        }
    }
}
