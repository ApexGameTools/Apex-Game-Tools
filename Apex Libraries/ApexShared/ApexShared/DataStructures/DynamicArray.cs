/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.DataStructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Ultra basic implementation of a dynamic array that forgoes most safety checks and relies on a certain usage pattern.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicArray<T> : IDynamicArray<T>, IIterable<T>, ISortable<T>
    {
        private static readonly T[] _empty = new T[0];

        private T[] _items;
        private int _capacity;
        private int _used;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicArray{T}"/> class.
        /// </summary>
        public DynamicArray()
        {
            _items = _empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicArray{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public DynamicArray(int capacity)
        {
            _items = new T[capacity];
            _capacity = capacity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicArray{T}"/> class.
        /// </summary>
        /// <param name="source">The source array.</param>
        public DynamicArray(T[] source)
        {
            _used = _capacity = source.Length;
            _items = new T[_capacity];
            Array.Copy(source, _items, _capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicArray{T}"/> class.
        /// </summary>
        /// <param name="source">The source list.</param>
        public DynamicArray(IIndexable<T> source)
        {
            _used = _capacity = source.count;
            _items = new T[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                _items[i] = source[i];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicArray{T}"/> class.
        /// </summary>
        /// <param name="source">The source list.</param>
        public DynamicArray(IEnumerable<T> source)
        {
            _items = source.ToArray();
            _used = _capacity = _items.Length;
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
            get { return _items[idx]; }

            set { _items[idx] = value; }
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
        }

        /// <summary>
        /// Adds the range of items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(T[] source, int start, int count)
        {
            int desiredLength = count + _used;
            if (_capacity < desiredLength)
            {
                Resize(desiredLength);
            }

            Array.Copy(source, start, _items, _used, count);
            _used = desiredLength;
        }

        public int IndexOf(Func<T, bool> predicate)
        {
            for (int i = _used - 1; i >= 0; i--)
            {
                if (predicate(_items[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool Contains(Func<T, bool> predicate)
        {
            return IndexOf(predicate) >= 0;
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

            for (int i = _used - 1; i >= 0; i--)
            {
                if (item.Equals(_items[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if found and removed; otherwise <c>false</c>
        /// </returns>
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
        /// Reorders the array such that an item is moved form one index to another and the rest of the array adapts to that.
        /// </summary>
        /// <param name="fromIdx">The from (source) index.</param>
        /// <param name="toIdx">The to (destination) index.</param>
        public void Reorder(int fromIdx, int toIdx)
        {
            var item = _items[fromIdx];

            if (fromIdx < toIdx)
            {
                for (int i = fromIdx + 1; i <= toIdx; i++)
                {
                    _items[i - 1] = _items[i];
                }
            }
            else
            {
                for (int i = fromIdx - 1; i >= toIdx; i--)
                {
                    _items[i + 1] = _items[i];
                }
            }

            _items[toIdx] = item;
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
        /// Sorts this instance.
        /// </summary>
        public void Sort()
        {
            Array.Sort(_items);
        }

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(_items, 0, _used, comparer);
        }

        /// <summary>
        /// Sorts a subset of this instance using the default comparer of its members.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        public void Sort(int index, int length)
        {
            Array.Sort(_items, index, length);
        }

        /// <summary>
        /// Sorts a subset of this instance using the specified comparer.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        /// <param name="comparer">The comparer.</param>
        public void Sort(int index, int length, IComparer<T> comparer)
        {
            if (index + length >= _used)
            {
                length = _used - index;
            }

            Array.Sort(_items, index, length, comparer);
        }

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(Comparison<T> comparer)
        {
            Array.Sort(_items, comparer);
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat("DynamicArray, count: ", this.count);
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