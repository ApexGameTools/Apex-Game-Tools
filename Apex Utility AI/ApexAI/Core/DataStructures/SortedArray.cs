/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.DataStructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Basic implementation of a sorted array that forgoes most safety checks and relies on a certain usage pattern.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortedArray<T> : IDynamicArray<T>, IIterable<T>
    {
        private static readonly T[] _empty = new T[0];

        private T[] _items;
        private int _capacity;
        private int _used;
        private IComparer<T> _sortComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedArray{T}"/> class.
        /// </summary>
        /// <param name="sortComparer">The sort comparer.</param>
        public SortedArray(IComparer<T> sortComparer)
        {
            _items = _empty;
            _sortComparer = sortComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedArray{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="sortComparer">The sort comparer.</param>
        public SortedArray(int capacity, IComparer<T> sortComparer)
        {
            _items = new T[capacity];
            _capacity = capacity;
            _sortComparer = sortComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedArray{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sortComparer">The sort comparer.</param>
        public SortedArray(T[] source, IComparer<T> sortComparer)
        {
            _used = _capacity = source.Length;
            _items = new T[_capacity];
            Array.Copy(source, _items, _capacity);

            _sortComparer = sortComparer;
            Array.Sort(_items, 0, _used, _sortComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedArray{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sortComparer">The sort comparer.</param>
        public SortedArray(IIndexable<T> source, IComparer<T> sortComparer)
        {
            _used = _capacity = source.count;
            _items = new T[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                _items[i] = source[i];
            }

            _sortComparer = sortComparer;
            Array.Sort(_items, 0, _used, _sortComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedArray{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sortComparer">The sort comparer.</param>
        public SortedArray(IEnumerable<T> source, IComparer<T> sortComparer)
        {
            _items = source.ToArray();
            _used = _capacity = _items.Length;

            _sortComparer = sortComparer;
            Array.Sort(_items, 0, _used, _sortComparer);
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
        /// Gets the value with the specified index. There is no bounds checking on get.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <returns>The value at the index</returns>
        public T this[int idx]
        {
            get
            {
                return _items[idx];
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            if (_used == _capacity)
            {
                int newCapacity = Math.Max(_capacity, 1);
                Resize(newCapacity * 2);
            }

            _items[_used++] = item;
            Array.Sort(_items, 0, _used, _sortComparer);
        }

        /// <summary>
        /// Adds the range of items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(IIndexable<T> items)
        {
            int desiredLength = items.count + _used;
            if (_capacity < desiredLength)
            {
                Resize(desiredLength);
            }

            for (int i = 0; i < items.count; i++)
            {
                _items[_used++] = items[i];
            }

            Array.Sort(_items, 0, _used, _sortComparer);
        }

        /// <summary>
        /// Adds the range of items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(IEnumerable<T> items)
        {
            int desiredLength = items.Count() + _used;
            if (_capacity < desiredLength)
            {
                Resize(desiredLength);
            }

            foreach (var item in items)
            {
                _items[_used++] = item;
            }

            Array.Sort(_items, 0, _used, _sortComparer);
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if found and removed; otherwise <c>false</c></returns>
        public bool Remove(T item)
        {
            for (int i = 0; i < _used; i++)
            {
                if (_items[i] != null && _items[i].Equals(item))
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            for (int i = index; i < _used - 1; i++)
            {
                _items[i] = _items[i + 1];
            }

            _items[_used - 1] = default(T);
            _used--;
        }

        /// <summary>
        /// Clears the array.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_items, 0, _used);
            _used = 0;
        }

        /// <summary>
        /// Ensures a certain capacity of the array, i.e. resizes the array to hold the specified number of items if not already able to.
        /// </summary>
        /// <param name="capacity">The capacity to ensure.</param>
        public void EnsureCapacity(int capacity)
        {
            if (_capacity < capacity)
            {
                Resize(capacity);
            }
        }

        private void Resize(int newCapacity)
        {
            _capacity = newCapacity;
            var tmp = new T[_capacity];
            Array.Copy(_items, 0, tmp, 0, _items.Length);
            _items = tmp;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                yield return _items[i];
            }
        }
    }
}