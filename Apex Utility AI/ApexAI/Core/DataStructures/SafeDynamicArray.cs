/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A wrapper around a <see cref="Apex.DataStructures.IDynamicArray{T}"/> which ensures that iteration never returns a null item.
    /// </summary>
    /// <typeparam name="T">The type stored</typeparam>
    /// <seealso cref="Apex.DataStructures.IDynamicArray{T}" />
    public sealed class SafeDynamicArray<T> : IDynamicArray<T>
    {
        private IDynamicArray<T> _array;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDynamicArray{T}"/> class.
        /// </summary>
        /// <param name="array">The array to wrap.</param>
        public SafeDynamicArray(IDynamicArray<T> array)
        {
            _array = array;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int count
        {
            get
            {
                for (int i = _array.count - 1; i >= 0; i--)
                {
                    if (_array[i] == null || _array[i].Equals(null))
                    {
                        _array.RemoveAt(i);
                    }
                }

                return _array.count;
            }
        }

        /// <summary>
        /// Gets the item at the specified index. This is not safe from nulls.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <returns>The item at the index</returns>
        public T this[int idx]
        {
            get { return _array[idx]; }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            _array.Add(item);
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if found and removed; otherwise <c>false</c></returns>
        public bool Remove(T item)
        {
            return _array.Remove(item);
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            _array.RemoveAt(index);
        }

        /// <summary>
        /// Clears the array.
        /// </summary>
        public void Clear()
        {
            _array.Clear();
        }

        /// <summary>
        /// Ensures the capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public void EnsureCapacity(int capacity)
        {
            _array.EnsureCapacity(capacity);
        }

        /// <summary>
        /// Gets the enumerator. This will skip any null items.
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var count = _array.count;
            for (int i = 0; i < count; i++)
            {
                if (_array[i] == null || _array[i].Equals(null))
                {
                    continue;
                }

                yield return _array[i];
            }
        }

        /// <summary>
        /// Gets the enumerator. This will skip any null items.
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var count = _array.count;
            for (int i = 0; i < count; i++)
            {
                if (_array[i] == null || _array[i].Equals(null))
                {
                    continue;
                }

                yield return _array[i];
            }
        }
    }
}
