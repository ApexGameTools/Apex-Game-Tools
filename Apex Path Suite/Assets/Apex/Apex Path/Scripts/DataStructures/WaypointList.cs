/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A list that keeps track of the actual waypoints of a path, including the intended current end of path
    /// </summary>
    public class WaypointList : IIterable<Vector3>
    {
        private Vector3[] _array;
        private int _used;
        private int _head;
        private int _tail;
        private int _current;
        private bool _frozen;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaypointList"/> class.
        /// </summary>
        public WaypointList()
            : this(4)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaypointList"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public WaypointList(int capacity)
        {
            capacity = Math.Max(capacity, 4);
            _array = new Vector3[capacity];
            _head = _tail = _current = -1;
            _used = 0;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int count
        {
            get { return _used; }
        }

        /// <summary>
        /// Gets a value indicating whether a pending waypoint exists.
        /// </summary>
        /// <value>
        /// <c>true</c> if a pending waypoint exists; otherwise, <c>false</c>.
        /// </value>
        public bool hasPendingWaypoint
        {
            get { return !_frozen && (_head != _current); }
        }

        /// <summary>
        /// Toggles whether this list is frozen or not. A frozen list will no longer serve up waypoints.
        /// </summary>
        /// <value>
        ///   <c>true</c> if frozen; otherwise, <c>false</c>.
        /// </value>
        public bool frozen
        {
            get { return _frozen; }
            set { _frozen = value; }
        }

        /// <summary>
        /// Gets the number of via points.
        /// </summary>
        public int viaPointsCount
        {
            get { return _current - _tail; }
        }

        /// <summary>
        /// Gets the desired end of the resolved path, that is the last waypoint that has been resolved.
        /// This may differ from the actual end of the path if the path was corrected due to blocked off areas etc.
        /// </summary>
        public Vector3 desiredEndOfPath
        {
            get { return _array[_current]; }
        }

        /// <summary>
        /// Gets the last waypoint
        /// </summary>
        /// <returns>The last waypoint.</returns>
        /// <exception cref="System.InvalidOperationException">The list is empty.</exception>
        public Vector3 last
        {
            get
            {
                if (_used == 0)
                {
                    throw new InvalidOperationException("The list is empty.");
                }

                return _array[_head];
            }
        }

        /// <summary>
        /// Gets the item with the specified index.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        /// <param name="idx">The index.</param>
        /// <returns>The item at the specified index</returns>
        public Vector3 this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= _used)
                {
                    throw new ArgumentOutOfRangeException("idx", "The list does not contain an item at that index.");
                }

                idx = (_tail + idx) % _array.Length;
                return _array[idx];
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_array, 0, _used);
            _used = 0;
            _head = _tail = _current = -1;
            _frozen = false;
        }

        /// <summary>
        /// Adds a waypoint
        /// </summary>
        /// <param name="wp">The waypoint.</param>
        /// <param name="asCurrent">If true the waypoint will be treated as the current waypoint, i.e. the current destination.</param>
        public void AddWaypoint(Vector3 wp, bool asCurrent = false)
        {
            if (_used == _array.Length)
            {
                var newArray = new Vector3[2 * _used];

                if (_tail < _head)
                {
                    Array.Copy(_array, _tail, newArray, 0, _used);
                }
                else
                {
                    Array.Copy(_array, _tail, newArray, 0, _used - _tail);
                    Array.Copy(_array, 0, newArray, _used - _tail, _head + 1);
                }

                _array = newArray;
                _tail = 0;
                _current = _head;
                _head = _used - 1;
            }
            else if (_tail < 0)
            {
                _tail = 0;
            }

            _head = (_head + 1) % _array.Length;

            _used++;
            _array[_head] = wp;

            if (asCurrent)
            {
                _current = _head;
            }
        }

        /// <summary>
        /// Gets the next waypoint in the list
        /// </summary>
        /// <returns>The next waypoint in the list</returns>
        /// <exception cref="System.InvalidOperationException">There is no next waypoint.</exception>
        public Vector3 NextWaypoint()
        {
            if (_head == _current)
            {
                throw new InvalidOperationException("There is no next waypoint.");
            }

            _current = (_current + 1) % _array.Length;
            return _array[_current];
        }

        /// <summary>
        /// Consumes a via point removing it from the list.
        /// </summary>
        public void ConsumeViaPoint()
        {
            if (_tail == _current)
            {
                return;
            }

            _array[_tail] = Vector3.zero;
            _tail = (_tail + 1) % _array.Length;
            _used--;
        }

        /// <summary>
        /// Gets the via points.
        /// </summary>
        /// <returns>The list of via points</returns>
        public Vector3[] GetViaPoints()
        {
            if (_tail == _current)
            {
                return null;
            }

            Vector3[] arr;
            if (_tail < _current)
            {
                var count = _current - _tail;
                arr = new Vector3[count];
                Array.Copy(_array, _tail, arr, 0, count);
            }
            else
            {
                var toEnd = _array.Length - _tail;
                arr = new Vector3[toEnd + _current];
                Array.Copy(_array, _tail, arr, 0, toEnd);
                Array.Copy(_array, 0, arr, toEnd, _current);
            }

            return arr;
        }

        /// <summary>
        /// Gets the pending waypoints. This will always include the end destination.
        /// </summary>
        /// <returns>The array of pending waypoints.</returns>
        public Vector3[] GetPending()
        {
            Vector3[] arr;
            if (_tail == _head)
            {
                arr = new Vector3[] { _array[_head] };
            }
            else if (_tail < _head)
            {
                var count = _head - _tail;
                arr = new Vector3[count];
                Array.Copy(_array, _tail + 1, arr, 0, count);
            }
            else
            {
                var toEnd = _array.Length - (_tail + 1);
                arr = new Vector3[toEnd + _head + 1];
                Array.Copy(_array, _tail + 1, arr, 0, toEnd);
                Array.Copy(_array, 0, arr, toEnd, _head + 1);
            }

            return arr;
        }

        IEnumerator<Vector3> IEnumerable<Vector3>.GetEnumerator()
        {
            for (int i = 0; i < _used; i++)
            {
                var idx = (_tail + i) % _array.Length;
                yield return _array[idx];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < _used; i++)
            {
                var idx = (_tail + i) % _array.Length;
                yield return _array[idx];
            }
        }
    }
}
