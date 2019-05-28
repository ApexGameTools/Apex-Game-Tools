/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A queue ADT that supports indexing.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class SimpleQueue<T> : IIterable<T>
    {
        private T[] _array;
        private int _used;
        private int _head;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQueue{T}"/> class.
        /// </summary>
        public SimpleQueue()
            : this(4)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public SimpleQueue(int capacity)
        {
            capacity = Math.Max(capacity, 4);
            _array = new T[capacity];
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
        /// Gets the item with the specified index.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        /// <param name="idx">The index.</param>
        /// <returns>The item at the specified index</returns>
        public T this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= _used)
                {
                    throw new ArgumentOutOfRangeException("idx", "The queue does not contain an item at that index.");
                }

                idx = (_head + idx) % _array.Length;
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
            _head = 0;
        }

        /// <summary>
        /// Returns the item at the front of the queue, but does not remove it.
        /// </summary>
        /// <returns>The item at the front of the queue.</returns>
        /// <exception cref="System.InvalidOperationException">The queue is empty.</exception>
        public T Peek()
        {
            if (_used == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            return _array[_head];
        }

        /// <summary>
        /// Returns the item at the back of queue.
        /// </summary>
        /// <returns>The item at the back of queue.</returns>
        /// <exception cref="System.InvalidOperationException">The queue is empty.</exception>
        public T Last()
        {
            if (_used == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            var idx = (_head + _used - 1) % _array.Length;
            return _array[idx];
        }

        public void RemoveAt(int idx)
        {
            if (idx < 0 || idx >= _used)
            {
                throw new ArgumentOutOfRangeException("idx", "The queue does not contain an item at that index.");
            }

            idx = (_head + idx) % _array.Length;
            _array[idx] = default(T);

            if (idx < _head)
            {
                var lastIdx = (_head + _used - 1) % _array.Length;
                for (int i = idx; i < lastIdx; i++)
                {
                    _array[i] = _array[i + 1];
                }

                _array[lastIdx] = default(T);
            }
            else
            {
                for (int i = idx; i > _head; i--)
                {
                    _array[i] = _array[i - 1];
                }

                _array[_head] = default(T);
                _head = (_head + 1) % _array.Length;
            }

            _used--;
        }

        /// <summary>
        /// Removes and returns the item at the front of the queue.
        /// </summary>
        /// <returns>The item at the front of the queue</returns>
        /// <exception cref="System.InvalidOperationException">The queue is empty.</exception>
        public T Dequeue()
        {
            if (_used == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            T t = _array[_head];
            _array[_head] = default(T);

            _used--;
            _head = (_head + 1) % _array.Length;

            return t;
        }

        /// <summary>
        /// Enters the specified item onto the back of the queue.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Enqueue(T item)
        {
            if (_used == _array.Length)
            {
                var newArray = new T[2 * _used];

                if (_head == 0)
                {
                    Array.Copy(_array, 0, newArray, 0, _used);
                }
                else
                {
                    Array.Copy(_array, _head, newArray, 0, _used - _head);
                    Array.Copy(_array, 0, newArray, _used - _head, _head);
                }

                _array = newArray;
                _head = 0;
            }

            var idx = (_head + _used) % _array.Length;

            _used++;
            _array[idx] = item;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public int IndexOf(T item)
        {
            if (item == null)
            {
                return -1;
            }

            for (int i = 0; i < _used; i++)
            {
                var idx = (_head + i) % _array.Length;
                if (item.Equals(_array[idx]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        public void Sort()
        {
            Array.Sort(_array);
            _head = 0;
        }

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(_array, 0, _used, comparer);
            _head = 0;
        }

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(Comparison<T> comparer)
        {
            Array.Sort(_array, comparer);
            _head = 0;
        }

        /// <summary>
        /// Converts the queue to an array.
        /// </summary>
        /// <returns>An array of elements in the same order as they were in the queue.</returns>
        public T[] ToArray()
        {
            var arr = new T[_used];
            if (_head + _used <= _array.Length)
            {
                Array.Copy(_array, _head, arr, 0, _used);
            }
            else
            {
                var toEnd = _array.Length - _head;
                Array.Copy(_array, _head, arr, 0, toEnd);
                Array.Copy(_array, 0, arr, toEnd, _used - toEnd);
            }

            return arr;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < _used; i++)
            {
                var idx = (_head + i) % _array.Length;
                yield return _array[idx];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < _used; i++)
            {
                var idx = (_head + i) % _array.Length;
                yield return _array[idx];
            }
        }
    }
}
