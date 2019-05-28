/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a Heap data structure.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class BinaryHeap<T> : BinaryHeapBase<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryHeap{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="comparer">The item comparer.</param>
        public BinaryHeap(int capacity, IComparer<T> comparer)
            : base(capacity, comparer)
        {
        }

        /// <summary>
        /// Enters an item in the heap.
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Add(T item)
        {
            AddInternal(item);
        }

        /// <summary>
        /// Removes the item at the front of the heap.
        /// </summary>
        /// <returns>The item at the front of the heap.</returns>
        public T Remove()
        {
            return RemoveInternal();
        }
    }
}
