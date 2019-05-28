/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections.Generic;

    /// <summary>
    /// Priority queue where same priority items will leave the queue in a LIFO manner
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class PriorityQueueLifo<T> : BinaryHeapBase<PriorityQueueLifo<T>.QueueItemLifo>
    {
        private int _entryIdx;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueueLifo{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="type">The queue type.</param>
        public PriorityQueueLifo(int capacity, QueueType type)
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
            this.AddInternal(new QueueItemLifo(item, priority, _entryIdx++));
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
        public struct QueueItemLifo
        {
            private T _item;
            private int _priority;
            private int _entryOrder;

            /// <summary>
            /// Initializes a new instance of the <see cref="QueueItemLifo"/> struct.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="priority">The priority.</param>
            /// <param name="entryOrder">The entry order.</param>
            public QueueItemLifo(T item, int priority, int entryOrder)
            {
                _item = item;
                _priority = priority;
                _entryOrder = entryOrder;
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

            /// <summary>
            /// Gets the entry order.
            /// </summary>
            /// <value>
            /// The entry order.
            /// </value>
            public int entryOrder
            {
                get { return _entryOrder; }
            }
        }

        private class ItemComparerMax : IComparer<QueueItemLifo>
        {
            public static readonly IComparer<QueueItemLifo> instance = new ItemComparerMax();

            public int Compare(QueueItemLifo x, QueueItemLifo y)
            {
                var prioCompare = x.priority.CompareTo(y.priority);
                if (prioCompare == 0)
                {
                    return x.entryOrder.CompareTo(y.entryOrder);
                }

                return prioCompare;
            }
        }

        private class ItemComparerMin : IComparer<QueueItemLifo>
        {
            public static readonly IComparer<QueueItemLifo> instance = new ItemComparerMin();

            public int Compare(QueueItemLifo x, QueueItemLifo y)
            {
                var prioCompare = y.priority.CompareTo(x.priority);
                if (prioCompare == 0)
                {
                    return x.entryOrder.CompareTo(y.entryOrder);
                }

                return prioCompare;
            }
        }
    }
}
