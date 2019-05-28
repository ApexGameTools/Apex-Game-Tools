/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a matrix of items.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class Matrix<T>
    {
        private T[,] _matrix;
        private int _columns;
        private int _rows;

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix{T}"/> class.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public Matrix(T[,] matrix)
        {
            _matrix = matrix;
            _columns = _matrix.GetUpperBound(0) + 1;
            _rows = _matrix.GetUpperBound(1) + 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix{T}"/> class.
        /// </summary>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        public Matrix(int columns, int rows)
        {
            _columns = columns;
            _rows = rows;

            _matrix = new T[_columns, _rows];
        }

        /// <summary>
        /// Gets the number of columns
        /// </summary>
        /// <value>
        /// The number of columns
        /// </value>
        public int columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets the number of rows
        /// </summary>
        /// <value>
        /// The number of rows
        /// </value>
        public int rows
        {
            get { return _rows; }
        }

        /// <summary>
        /// Gets the raw matrix. No bounds checking will be done on access
        /// </summary>
        /// <value>
        /// The raw matrix.
        /// </value>
        public T[,] rawMatrix
        {
            get { return _matrix; }
        }

        /// <summary>
        /// Gets the items in the matrix
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public IEnumerable<T> items
        {
            get
            {
                for (int x = 0; x < _columns; x++)
                {
                    for (int z = 0; z < _rows; z++)
                    {
                        yield return _matrix[x, z];
                    }
                }
            }
        }

        /// <summary>
        /// Gets the item at the specified position in the matrix. Bounds check is performed and will return null if out of bounds.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <returns>If the position is valid (in bounds) the item at that position is returned. Otherwise null is returned.</returns>
        public T this[int column, int row]
        {
            get
            {
                if (InBounds(column, row))
                {
                    return _matrix[column, row];
                }

                return default(T);
            }
        }

        /// <summary>
        /// Gets a layer of items around a center. A layer is defined as the outer neighbours of the concentric square given by the cellDistance argument.
        /// Distance of 0 is the item itself, Distance of 1 is the 8 neighbouring items, Distance of 2 is the 16  outer most neighboring items to layer 1, etc. Think onion.
        /// </summary>
        /// <param name="column">The column from which to find neighbours</param>
        /// <param name="row">The row from which to find neighbours</param>
        /// <param name="cellDistance">The cell distance, 0 being the item itself.</param>
        /// <param name="neighbours">A dynamic array to populate with the found neighbour cells.</param>
        /// <returns>The list of items making up the layer.</returns>
        public void GetConcentricNeighbours(int column, int row, int cellDistance, DynamicArray<T> neighbours)
        {
            if (cellDistance < 0)
            {
                return;
            }

            if (cellDistance == 0)
            {
                neighbours.Add(this[column, row]);
                return;
            }

            var startColumnRaw = column - cellDistance;
            var endColumnRaw = column + cellDistance;

            var startRowRaw = row - cellDistance;
            var endRowRaw = row + cellDistance;

            var startColumnBounded = AdjustColumnToBounds(startColumnRaw);
            var endColumnBounded = AdjustColumnToBounds(endColumnRaw);

            var startRowBounded = AdjustRowToBounds(startRowRaw + 1);
            var endRowBounded = AdjustRowToBounds(endRowRaw - 1);

            for (int x = startColumnBounded; x <= endColumnBounded; x++)
            {
                if (startRowRaw >= 0)
                {
                    neighbours.Add(_matrix[x, startRowRaw]);
                }

                if (endRowRaw < _rows)
                {
                    neighbours.Add(_matrix[x, endRowRaw]);
                }
            }

            for (int z = startRowBounded; z <= endRowBounded; z++)
            {
                if (startColumnRaw >= 0)
                {
                    neighbours.Add(_matrix[startColumnRaw, z]);
                }

                if (endColumnRaw < _columns)
                {
                    neighbours.Add(_matrix[endColumnRaw, z]);
                }
            }
        }

        /// <summary>
        /// Gets a layer of items around a center. A layer is defined as the outer neighbours of the concentric square given by the cellDistance argument.
        /// Distance of 0 is the item itself, Distance of 1 is the 8 neighbouring items, Distance of 2 is the 16  outer most neighboring items to layer 1, etc. Think onion.
        /// </summary>
        /// <param name="column">The column from which to find neighbours</param>
        /// <param name="row">The row from which to find neighbours</param>
        /// <param name="cellDistance">The cell distance, 0 being the item itself.</param>
        /// <returns>The list of items making up the layer.</returns>
        public IEnumerable<T> GetConcentricNeighbours(int column, int row, int cellDistance)
        {
            if (cellDistance < 0)
            {
                yield break;
            }

            if (cellDistance == 0)
            {
                yield return this[column, row];
                yield break;
            }

            var startColumnRaw = column - cellDistance;
            var endColumnRaw = column + cellDistance;

            var startRowRaw = row - cellDistance;
            var endRowRaw = row + cellDistance;

            var startColumnBounded = AdjustColumnToBounds(startColumnRaw);
            var endColumnBounded = AdjustColumnToBounds(endColumnRaw);

            var startRowBounded = AdjustRowToBounds(startRowRaw + 1);
            var endRowBounded = AdjustRowToBounds(endRowRaw - 1);

            for (int x = startColumnBounded; x <= endColumnBounded; x++)
            {
                if (startRowRaw >= 0)
                {
                    yield return _matrix[x, startRowRaw];
                }

                if (endRowRaw < _rows)
                {
                    yield return _matrix[x, endRowRaw];
                }
            }

            for (int z = startRowBounded; z <= endRowBounded; z++)
            {
                if (startColumnRaw >= 0)
                {
                    yield return _matrix[startColumnRaw, z];
                }

                if (endColumnRaw < _columns)
                {
                    yield return _matrix[endColumnRaw, z];
                }
            }
        }

        /// <summary>
        /// Gets a range of items
        /// </summary>
        /// <param name="bounds">The bounds specifying the index range.</param>
        /// <returns>The range of items that lie inside the index range given by the parameter.</returns>
        public IEnumerable<T> GetRange(MatrixBounds bounds)
        {
            return GetRange(bounds.minColumn, bounds.maxColumn, bounds.minRow, bounds.maxRow);
        }

        /// <summary>
        /// Gets a range of items
        /// </summary>
        /// <param name="fromColumn">From column.</param>
        /// <param name="toColumn">To column.</param>
        /// <param name="fromRow">From row.</param>
        /// <param name="toRow">To row.</param>
        /// <returns>The range of items that lie in between and including the given parameters.</returns>
        public IEnumerable<T> GetRange(int fromColumn, int toColumn, int fromRow, int toRow)
        {
            var startColumn = AdjustColumnToBounds(fromColumn);
            var endColumn = AdjustColumnToBounds(toColumn);

            var startRow = AdjustRowToBounds(fromRow);
            var endRow = AdjustRowToBounds(toRow);

            for (int x = startColumn; x <= endColumn; x++)
            {
                for (int z = startRow; z <= endRow; z++)
                {
                    yield return _matrix[x, z];
                }
            }
        }

        /// <summary>
        /// Gets a range of items
        /// </summary>
        /// <param name="bounds">The bounds specifying the index range.</param>
        /// <param name="result">The result to be populated by the range of items that lie inside the index range given by the parameter.</param>
        public void GetRange(MatrixBounds bounds, ICollection<T> result)
        {
            GetRange(bounds.minColumn, bounds.maxColumn, bounds.minRow, bounds.maxRow, result);
        }

        /// <summary>
        /// Gets a range of items
        /// </summary>
        /// <param name="bounds">The bounds specifying the index range.</param>
        /// <param name="predicate">A filter predicate, only those elements that match the predicate will be included in the result.</param>
        /// <param name="result">The result to be populated by the range of items that lie inside the index range given by the parameter.</param>
        public void GetRange(MatrixBounds bounds, Func<T, bool> predicate, ICollection<T> result)
        {
            GetRange(bounds.minColumn, bounds.maxColumn, bounds.minRow, bounds.maxRow, predicate, result);
        }

        /// <summary>
        /// Gets a range of items
        /// </summary>
        /// <param name="fromColumn">From column.</param>
        /// <param name="toColumn">To column.</param>
        /// <param name="fromRow">From row.</param>
        /// <param name="toRow">To row.</param>
        /// <param name="result">The result to be populated by the range of items that lie in between and including the given parameters.</param>
        public void GetRange(int fromColumn, int toColumn, int fromRow, int toRow, ICollection<T> result)
        {
            var startColumn = AdjustColumnToBounds(fromColumn);
            var endColumn = AdjustColumnToBounds(toColumn);

            var startRow = AdjustRowToBounds(fromRow);
            var endRow = AdjustRowToBounds(toRow);

            for (int x = startColumn; x <= endColumn; x++)
            {
                for (int z = startRow; z <= endRow; z++)
                {
                    result.Add(_matrix[x, z]);
                }
            }
        }

        /// <summary>
        /// Gets a range of items
        /// </summary>
        /// <param name="fromColumn">From column.</param>
        /// <param name="toColumn">To column.</param>
        /// <param name="fromRow">From row.</param>
        /// <param name="toRow">To row.</param>
        /// <param name="predicate">A filter predicate, only those elements that match the predicate will be included in the result.</param>
        /// <param name="result">The result to be populated by the range of items that lie in between and including the given parameters.</param>
        public void GetRange(int fromColumn, int toColumn, int fromRow, int toRow, Func<T, bool> predicate, ICollection<T> result)
        {
            var startColumn = AdjustColumnToBounds(fromColumn);
            var endColumn = AdjustColumnToBounds(toColumn);

            var startRow = AdjustRowToBounds(fromRow);
            var endRow = AdjustRowToBounds(toRow);

            for (int x = startColumn; x <= endColumn; x++)
            {
                for (int z = startRow; z <= endRow; z++)
                {
                    var item = _matrix[x, z];
                    if (predicate(item))
                    {
                        result.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Applies an action to a range of items
        /// </summary>
        /// <param name="bounds">The bounds specifying the index range.</param>
        /// <param name="act">The action to apply. This can be anything from modifying the items to collecting data from the items.</param>
        public void Apply(MatrixBounds bounds, Action<T> act)
        {
            Apply(bounds.minColumn, bounds.maxColumn, bounds.minRow, bounds.maxRow, act);
        }

        /// <summary>
        /// Applies an action to a range of items
        /// </summary>
        /// <param name="fromColumn">From column.</param>
        /// <param name="toColumn">To column.</param>
        /// <param name="fromRow">From row.</param>
        /// <param name="toRow">To row.</param>
        /// <param name="act">The action to apply. This can be anything from modifying the items to collecting data from the items.</param>
        public void Apply(int fromColumn, int toColumn, int fromRow, int toRow, Action<T> act)
        {
            var startColumn = AdjustColumnToBounds(fromColumn);
            var endColumn = AdjustColumnToBounds(toColumn);

            var startRow = AdjustRowToBounds(fromRow);
            var endRow = AdjustRowToBounds(toRow);

            for (int x = startColumn; x <= endColumn; x++)
            {
                for (int z = startRow; z <= endRow; z++)
                {
                    act(_matrix[x, z]);
                }
            }
        }

        /// <summary>
        /// Are the matrix indexes within the bounds of the matrix.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="z">The z.</param>
        /// <returns><c>true if in bounds; otherwise false</c></returns>
        protected bool InBounds(int x, int z)
        {
            if (x < 0 || x > _columns - 1)
            {
                return false;
            }

            if (z < 0 || z > _rows - 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adjusts the column to bounds.
        /// </summary>
        /// <param name="x">The column index.</param>
        /// <returns>The index adjusted to bounds</returns>
        protected int AdjustColumnToBounds(int x)
        {
            if (x < 0)
            {
                return 0;
            }

            if (x > _columns - 1)
            {
                return _columns - 1;
            }

            return x;
        }

        /// <summary>
        /// Adjusts the row to bounds.
        /// </summary>
        /// <param name="z">The row index.</param>
        /// <returns>The index adjusted to bounds</returns>
        protected int AdjustRowToBounds(int z)
        {
            if (z < 0)
            {
                return 0;
            }

            if (z > _rows - 1)
            {
                return _rows - 1;
            }

            return z;
        }
    }
}
