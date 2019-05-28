/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections.Generic;

    /// <summary>
    /// Basic priority queue, no entry order semantics
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class PriorityQueue<T> : BinaryHeapBase<PriorityQueue<T>.QueueItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="type">The type of queue.</param>
        public PriorityQueue(int capacity, QueueType type)
            : base(capacity, type == QueueType.Max ? ItemComparerMax.instance : ItemComparerMin.instance)
        {
        }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="priority">The priority.</param>
        public void Enqueue(T item, int priority)
        {
            this.AddInternal(new QueueItem(item, priority));
        }

        /// <summary>
        /// Dequeues the next item in the queue.
        /// </summary>
        /// <returns>The item</returns>
        public T Dequeue()
        {
            return this.RemoveInternal().item;
        }

        /// <summary>
        /// Wraps each item in the queue.
        /// </summary>
        public struct QueueItem
        {
            private T _item;
            private int _priority;

            /// <summary>
            /// Initializes a new instance of the <see cref="QueueItem"/> struct.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="priority">The priority.</param>
            public QueueItem(T item, int priority)
            {
                _item = item;
                _priority = priority;
            }

            /// <summary>
            /// Gets the item.
            /// </summary>
            /// <value>
            /// The item.
            /// </value>
            public T item
            {
                get { return _item; }
            }

            /// <summary>
            /// Gets the priority.
            /// </summary>
            /// <value>
            /// The priority.
            /// </value>
            public int priority
            {
                get { return _priority; }
            }
        }

        private class ItemComparerMax : IComparer<QueueItem>
        {
            public static readonly IComparer<QueueItem> instance = new ItemComparerMax();

            public int Compare(QueueItem x, QueueItem y)
            {
                return x.priority.CompareTo(y.priority);
            }
        }

        private class ItemComparerMin : IComparer<QueueItem>
        {
            public static readonly IComparer<QueueItem> instance = new ItemComparerMin();

            public int Compare(QueueItem x, QueueItem y)
            {
                return y.priority.CompareTo(x.priority);
            }
        }
    }
}
