/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections.Generic;

    /// <summary>
    /// Priority queue where same priority items will leave the queue in a FIFO manner
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class PriorityQueueFifo<T> : BinaryHeapBase<PriorityQueueFifo<T>.QueueItemFifo>
    {
        private int _entryIdx;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueueFifo{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="type">The type of queue.</param>
        public PriorityQueueFifo(int capacity, QueueType type)
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
            this.AddInternal(new QueueItemFifo(item, priority, _entryIdx++));
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
        public struct QueueItemFifo
        {
            private T _item;
            private int _priority;
            private int _entryOrder;

            /// <summary>
            /// Initializes a new instance of the <see cref="QueueItemFifo"/> struct.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="priority">The priority.</param>
            /// <param name="entryOrder">The entry order.</param>
            public QueueItemFifo(T item, int priority, int entryOrder)
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

        private class ItemComparerMax : IComparer<QueueItemFifo>
        {
            public static readonly IComparer<QueueItemFifo> instance = new ItemComparerMax();

            public int Compare(QueueItemFifo x, QueueItemFifo y)
            {
                var prioCompare = x.priority.CompareTo(y.priority);
                if (prioCompare == 0)
                {
                    return y.entryOrder.CompareTo(x.entryOrder);
                }

                return prioCompare;
            }
        }

        private class ItemComparerMin : IComparer<QueueItemFifo>
        {
            public static readonly IComparer<QueueItemFifo> instance = new ItemComparerMin();

            public int Compare(QueueItemFifo x, QueueItemFifo y)
            {
                var prioCompare = y.priority.CompareTo(x.priority);
                if (prioCompare == 0)
                {
                    return y.entryOrder.CompareTo(x.entryOrder);
                }

                return prioCompare;
            }
        }
    }
}
