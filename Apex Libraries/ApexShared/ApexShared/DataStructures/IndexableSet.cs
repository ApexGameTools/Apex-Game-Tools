namespace Apex.DataStructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex.Utilities;

    /// <summary>
    /// A combination of a list and a hashset. Lookups are O(1) and the data structure is indexable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexableSet<T> : IDynamicArray<T>, IIterable<T>, ISortable<T>
    {
        private HashSet<T> _hashset;
        private DynamicArray<T> _array;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexableSet{T}"/> class.
        /// </summary>
        public IndexableSet()
        {
            _hashset = new HashSet<T>();
            _array = new DynamicArray<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexableSet{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public IndexableSet(int capacity)
        {
            Ensure.ArgumentInRange(() => capacity >= 0, "capacity", capacity);

            _hashset = new HashSet<T>();
            _array = new DynamicArray<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexableSet{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public IndexableSet(params T[] items)
        {
            Ensure.ArgumentNotNull(items, "items");

            _hashset = new HashSet<T>(items);
            _array = new DynamicArray<T>(items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexableSet{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public IndexableSet(IEnumerable<T> items)
        {
            Ensure.ArgumentNotNull(items, "items");

            _hashset = new HashSet<T>(items);
            _array = new DynamicArray<T>(items);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int count
        {
            get { return _hashset.Count; }
        }

        /// <summary>
        /// Gets the value with the specified index.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <returns>The value at the index</returns>
        public T this[int idx]
        {
            get { return _array[idx]; }
        }

        /// <summary>
        /// Adds the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Add(T obj)
        {
            if (_hashset.Add(obj))
            {
                _array.Add(obj);
            }
        }

        /// <summary>
        /// Adds the range of objects.
        /// </summary>
        /// <param name="objects">The objects.</param>
        public void AddRange(params T[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                Add(objects[i]);
            }
        }

        /// <summary>
        /// Adds the range of objects.
        /// </summary>
        /// <param name="objects">The objects.</param>
        public void AddRange(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Adds the range of objects.
        /// </summary>
        /// <param name="objects">The objects.</param>
        public void AddRange(IIndexable<T> objects)
        {
            for (int i = 0; i < objects.count; i++)
            {
                Add(objects[i]);
            }
        }

        /// <summary>
        /// Removes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if the object was found and removed; otherwise <c>false</c></returns>
        public bool Remove(T obj)
        {
            if (_hashset.Remove(obj))
            {
                _array.Remove(obj);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            var obj = _array[index];
            _array.RemoveAt(index);
            _hashset.Remove(obj);
        }

        /// <summary>
        /// Determines whether the set contains the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if the object is contained in the set; otherwise <c>false</c></returns>
        public bool Contains(T obj)
        {
            return _hashset.Contains(obj);
        }

        /// <summary>
        /// Sorts this instance using the default comparer of its members.
        /// </summary>
        public void Sort()
        {
            _array.Sort();
        }

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer<T> comparer)
        {
            _array.Sort(comparer);
        }

        /// <summary>
        /// Sorts a subset of this instance using the default comparer of its members.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        public void Sort(int index, int length)
        {
            _array.Sort(index, length);
        }

        /// <summary>
        /// Sorts a subset of this instance using the specified comparer.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        /// <param name="comparer">The comparer.</param>
        public void Sort(int index, int length, IComparer<T> comparer)
        {
            _array.Sort(index, length, comparer);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _array.Clear();
            _hashset.Clear();
        }

        /// <summary>
        /// Ensures a certain capacity of the array, i.e. resizes the array to hold the specified number of items if not already able to.
        /// </summary>
        /// <param name="capacity">The capacity to ensure.</param>
        public void EnsureCapacity(int capacity)
        {
            //We can only ensure capacity on the dynamic array ince hash sets have no such option.
            _array.EnsureCapacity(capacity);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                yield return _array[i];
            }
        }
    }
}