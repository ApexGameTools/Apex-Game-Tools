/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591

namespace Apex.Common
{
    using System;
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.WorldGeometry;
    using UnityEngine;

    public class GridEvaluatorQueueBased
    {
        private SimpleQueue<Cell> _queue;
        private HashSet<Cell> _set;
        private Cell[] _buffer;

        public GridEvaluatorQueueBased(int bufferInitialSize)
        {
            _queue = new SimpleQueue<Cell>(bufferInitialSize);
            _set = new HashSet<Cell>();
            _buffer = new Cell[8];
        }

        public Cell BestMatch(IGrid grid, Vector3 start, Func<Cell, bool> match, Func<Cell, bool> discard)
        {
            var startCell = grid.GetCell(start, true);
            if (startCell == null)
            {
                return null;
            }

            _set.Add(startCell);
            _queue.Enqueue(startCell);

            while (_queue.count > 0)
            {
                var current = _queue.Dequeue();

                var count = current.GetNeighbours(_buffer);
                for (int i = 0; i < count; i++)
                {
                    var n = _buffer[i];
                    if (!_set.Add(n))
                    {
                        continue;
                    }

                    if (match(n))
                    {
                        Reset();
                        return n;
                    }

                    if (!discard(n))
                    {
                        _queue.Enqueue(n);
                    }
                }
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
    }
}
