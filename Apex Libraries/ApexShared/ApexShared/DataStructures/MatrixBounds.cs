/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;

    /// <summary>
    /// Represents a matrix boundary consisting of min/max values for matrix indexes.
    /// </summary>
    public struct MatrixBounds
    {
        /// <summary>
        /// Represent an empty bounds
        /// </summary>
        public static readonly MatrixBounds nullBounds = new MatrixBounds(-1, -1, -2, -2);

        /// <summary>
        /// The minimum column index
        /// </summary>
        public int minColumn;

        /// <summary>
        /// The maximum column index
        /// </summary>
        public int maxColumn;

        /// <summary>
        /// The minimum row index
        /// </summary>
        public int minRow;

        /// <summary>
        /// The maximum row index
        /// </summary>
        public int maxRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixBounds"/> struct.
        /// </summary>
        /// <param name="minColumn">The minimum column index.</param>
        /// <param name="minRow">The minimum row index.</param>
        /// <param name="maxColumn">The maximum column index.</param>
        /// <param name="maxRow">The maximum row index.</param>
        public MatrixBounds(int minColumn, int minRow, int maxColumn, int maxRow)
        {
            this.minColumn = minColumn;
            this.minRow = minRow;
            this.maxColumn = maxColumn;
            this.maxRow = maxRow;
        }

        /// <summary>
        /// Gets a value indicating whether this bounds is empty, i.e. min &gt; max.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool isEmpty
        {
            get { return (this.minColumn > this.maxColumn) || (this.minRow > this.maxRow); }
        }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public int columns
        {
            get { return this.maxColumn - this.minColumn + 1; }
        }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        /// <value>
        /// The rows.
        /// </value>
        public int rows
        {
            get { return this.maxRow - this.minRow + 1; }
        }

        /// <summary>
        /// Combines two bounds to create a new bounds that covers the area of both plus any area between them.
        /// If one <see cref="isEmpty"/> it will return the other.
        /// </summary>
        /// <param name="first">The first bounds.</param>
        /// <param name="second">The second bounds.</param>
        /// <returns>A new bounds that covers the area of both plus any area between them.</returns>
        public static MatrixBounds Combine(MatrixBounds first, MatrixBounds second)
        {
            if (first.isEmpty)
            {
                return second;
            }

            if (second.isEmpty)
            {
                return first;
            }

            return new MatrixBounds(
                Math.Min(first.minColumn, second.minColumn),
                Math.Min(first.minRow, second.minRow),
                Math.Max(first.maxColumn, second.maxColumn),
                Math.Max(first.maxRow, second.maxRow));
        }

        /// <summary>
        /// Adjusts the column to bounds.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The index adjusted to bounds</returns>
        public int AdjustColumnToBounds(int column)
        {
            if (column < this.minColumn)
            {
                return this.minColumn;
            }

            if (column > this.maxColumn)
            {
                return this.maxColumn;
            }

            return column;
        }

        /// <summary>
        /// Adjusts the row to bounds.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>The index adjusted to bounds</returns>
        public int AdjustRowToBounds(int row)
        {
            if (row < this.minRow)
            {
                return this.minRow;
            }

            if (row > this.maxRow)
            {
                return this.maxRow;
            }

            return row;
        }

        /// <summary>
        /// Determines whether the specified matrix coordinates are contained in this bounds.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <returns><c>true</c> if contained; otherwise <c>false</c></returns>
        public bool Contains(int column, int row)
        {
            return (column >= this.minColumn) &&
                    (column <= this.maxColumn) &&
                    (row >= this.minRow) &&
                    (row <= this.maxRow);
        }

        /// <summary>
        /// Determines whether this bounds contains the other bounds.
        /// </summary>
        /// <param name="other">The other bounds.</param>
        /// <returns><c>true</c> if contained; otherwise <c>false</c></returns>
        public bool Contains(MatrixBounds other)
        {
            return ((other.maxRow <= this.maxRow) && (other.minRow >= this.minRow) && (other.maxColumn <= this.maxColumn) && (other.minColumn >= this.minColumn));
        }

        /// <summary>
        /// If looking at the matrix bounds as an array (see below) this will return the index in this array that a given pair of column,row values would represent.
        ///  _______________
        /// |_1_|_3_|_5_|_7_|
        /// |_0_|_2_|_4_|_6_|
        /// How a matrix bounds is represented as an array
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public int IndexOf(int column, int row)
        {
            if (column < this.minColumn || column > this.maxColumn || row < this.minRow || row > this.maxRow)
            {
                return -1;
            }

            return ((column - this.minColumn) * this.rows) + (row - this.minRow);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("xmin: {0}, zmin: {1}, xmax: {2}, zmax: {3}", this.minColumn, this.minRow, this.maxColumn, this.maxRow);
        }
    }
}
