/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections.Generic;

    /// <summary>
    /// A queue that only lets a given item be queued once in the lifetime of the queue. Only use this as an intermediary structure to facilitate sampling.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class QueueOnce<T>
    {
        private HashSet<T> _set = new HashSet<T>();
        private Queue<T> _q = new Queue<T>();

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int count
        {
            get { return _q.Count; }
        }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the item was added, i.e. has not already been added before, otherwise <c>false</c></returns>
        public bool Enqueue(T item)
        {
            if (_set.Add(item))
            {
                _q.Enqueue(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Enqueues the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void Enqueue(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (_set.Add(item))
                {
                    _q.Enqueue(item);
                }
            }
        }

        /// <summary>
        /// Dequeues the next item in the queue.
        /// </summary>
        /// <returns>The item</returns>
        public T Dequeue()
        {
            return _q.Dequeue();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _q.Clear();
        }

        /// <summary>
        /// Determines whether the specified item has been queued before.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the item has not already been added before, otherwise <c>false</c></returns>
        public bool HasQueued(T item)
        {
            return _set.Contains(item);
        }
    }
}
