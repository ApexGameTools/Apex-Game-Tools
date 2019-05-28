/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Common
{
    using System;
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Utility class that evaluates a grid to find cells that meet certain requirements.
    /// </summary>
    public class GridEvaluatorClosestMatch
    {
        private BinaryHeap<CellItem> _queue;
        private HashSet<Cell> _set;
        private Cell[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridEvaluatorClosestMatch"/> class.
        /// </summary>
        /// <param name="bufferInitialSize">Initial size of the buffer.</param>
        public GridEvaluatorClosestMatch(int bufferInitialSize)
        {
            _queue = new BinaryHeap<CellItem>(bufferInitialSize, ItemComparerMin.instance);
            _set = new HashSet<Cell>();
            _buffer = new Cell[8];
        }

        /// <summary>
        /// Find the closest cell to a given point that matches the specified requirements.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="start">The start point.</param>
        /// <param name="match">The function representing the requirements.</param>
        /// <param name="discard">The function that determines if a cell should be discarded, e.g. blocked cells.</param>
        /// <returns>The closest match or null if no cell is found.</returns>
        public Cell ClosestMatch(IGrid grid, Vector3 start, Func<Cell, bool> match, Func<Cell, bool> discard)
        {
            var startCell = grid.GetCell(start, true);

            _set.Add(startCell);

            var refPos = start;
            var curCell = startCell;
            var current = new CellItem
            {
                cell = startCell,
                priority = 0f
            };

            while (true)
            {
                if (match(curCell))
                {
                    Reset();
                    return curCell;
                }

                var count = curCell.GetNeighbours(_buffer);
                for (int i = 0; i < count; i++)
                {
                    var n = _buffer[i];
                    if (!_set.Add(n))
                    {
                        continue;
                    }

                    if (!discard(n))
                    {
                        var item = new CellItem
                        {
                            cell = n,
                            priority = current.priority + (n.position - refPos).sqrMagnitude
                        };

                        _queue.Add(item);
                    }
                }

                if (_queue.count == 0)
                {
                    break;
                }

                current = _queue.Remove();
                curCell = current.cell;
                refPos = curCell.position;
            }

            Reset();
            return null;
        }

        private void Reset()
        {
            _set.Clear();
            _queue.Clear();
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        private struct CellItem
        {
            internal Cell cell;
            internal float priority;
        }

        private class ItemComparerMin : IComparer<CellItem>
        {
            public static readonly IComparer<CellItem> instance = new ItemComparerMin();

            public int Compare(CellItem x, CellItem y)
            {
                return y.priority.CompareTo(x.priority);
            }
        }
    }
}
