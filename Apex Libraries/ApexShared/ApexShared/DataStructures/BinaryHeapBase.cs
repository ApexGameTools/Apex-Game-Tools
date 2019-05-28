/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.DataStructures
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a Heap data structure.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public abstract class BinaryHeapBase<T>
    {
        private int _used;
        private T[] _heap;
        private IComparer<T> _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryHeapBase{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="comparer">The comparer used to compare the item in the heap.</param>
        protected BinaryHeapBase(int capacity, IComparer<T> comparer)
        {
            if (capacity < 4)
            {
                capacity = 4;
            }

            _heap = new T[capacity];
            _comparer = comparer;
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
        /// Gets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        public int capacity
        {
            get { return _heap.Length; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any items.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has at least one item; otherwise, <c>false</c>.
        /// </value>
        public bool hasNext
        {
            get { return (_used > 0); }
        }

        /// <summary>
        /// Gets the item at the front of the heap, but does not remove it.
        /// </summary>
        /// <returns>The item at the front of the heap.</returns>
        public T Peek()
        {
            if (_used == 0)
            {
                throw new InvalidOperationException("The Heap is empty, Peek cannot be called on an empty heap");
            }

            return _heap[0];
        }

        /// <summary>
        /// Clears the heap, removing all items.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_heap, 0, _used);
            _used = 0;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The item that was removed or null if it was not found.</returns>
        public T Remove(T item)
        {
            //Find the item
            int childIdx = Array.IndexOf(_heap, item, 0);
            if (childIdx < 0)
            {
                return default(T);
            }

            return Remove(childIdx);
        }

        /// <summary>
        /// Removes the first item that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The item that was removed or null if no item was not found.</returns>
        public T Remove(Func<T, bool> predicate)
        {
            //Find the item
            int childIdx = -1;
            for (int i = 0; i < _used; i++)
            {
                if (predicate(_heap[i]))
                {
                    childIdx = i;
                    break;
                }
            }

            if (childIdx < 0)
            {
                return default(T);
            }

            return Remove(childIdx);
        }

        /// <summary>
        /// Recreates the heap upwards from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void ReheapifyUpFrom(T item)
        {
            //Find the item, skipping the front of the queue since if its already there its not going any more up :)
            int childIdx = Array.IndexOf(_heap, item, 1);
            if (childIdx < 1)
            {
                return;
            }

            ReheapifyUp(childIdx);
        }

        /// <summary>
        /// Recreates the heap downwards from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void ReheapifyDownFrom(T item)
        {
            //Find the item
            int childIdx = Array.IndexOf(_heap, item, 0);
            if (childIdx < 0)
            {
                return;
            }

            ReheapifyDown(childIdx);
        }

        /// <summary>
        /// Recreates the heap downwards from the specified index.
        /// </summary>
        /// <param name="childIdx">Index of the child.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">childIdx;Specified index is outside the valid range.</exception>
        public void ReheapifyDownFrom(int childIdx)
        {
            if (childIdx < 0 || childIdx >= _heap.Length)
            {
                throw new ArgumentOutOfRangeException("childIdx", "Specified index is outside the valid range.");
            }

            ReheapifyDown(childIdx);
        }

        /// <summary>
        /// Resizes this instance by doubling its capacity.
        /// </summary>
        public void Resize()
        {
            Resize(_heap.Length * 2);
        }

        /// <summary>
        /// Resizes this instance to the specified capacity, if greater than the current capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public void Resize(int capacity)
        {
            if (capacity <= _used)
            {
                return;
            }

            var newHeap = new T[capacity];

            if (_used > 0)
            {
                Array.Copy(_heap, newHeap, _used);
            }

            _heap = newHeap;
        }

        /// <summary>
        /// Enters an item in the heap.
        /// </summary>
        /// <param name="item">The item to add</param>
        protected void AddInternal(T item)
        {
            if (_used == _heap.Length)
            {
                Resize();
            }

            _heap[_used++] = item;

            ReheapifyUp(_used - 1);
        }

        /// <summary>
        /// Removes the item at the front of the heap.
        /// </summary>
        /// <returns>The item at the front of the heap.</returns>
        protected T RemoveInternal()
        {
            return Remove(0);
        }

        private T Remove(int idx)
        {
            if (_used == 0)
            {
                throw new InvalidOperationException("The Heap is empty, Remove cannot be called on an empty heap");
            }

            T value = _heap[idx];
            _used--;

            _heap[idx] = _heap[_used];
            _heap[_used] = default(T);

            ReheapifyDown(idx);

            return value;
        }

        private void ReheapifyUp(int childIdx)
        {
            int parentIdx = (childIdx - 1) / 2;
            var item = _heap[childIdx];

            while ((childIdx > 0) && (_comparer.Compare(item, _heap[parentIdx]) > 0))
            {
                _heap[childIdx] = _heap[parentIdx];

                childIdx = parentIdx;
                parentIdx = (childIdx - 1) / 2;
            }

            _heap[childIdx] = item;
        }

        private void ReheapifyDown(int currentIdx)
        {
            int childIdx;
            T item = _heap[currentIdx];

            while (true)
            {
                int leftChildIdx = (currentIdx * 2) + 1;
                if (leftChildIdx >= _used)
                {
                    break;
                }

                int rightChildIdx = (currentIdx * 2) + 2;
                if (rightChildIdx >= _used)
                {
                    childIdx = leftChildIdx;
                }
                else
                {
                    childIdx = (_comparer.Compare(_heap[leftChildIdx], _heap[rightChildIdx]) > 0) ? leftChildIdx : rightChildIdx;
                }

                if (_comparer.Compare(item, _heap[childIdx]) < 0)
                {
                    _heap[currentIdx] = _heap[childIdx];
                    currentIdx = childIdx;
                }
                else
                {
                    break;
                }
            }

            _heap[currentIdx] = item;
        }
    }
}