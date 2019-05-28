/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Pooling of list buffers. This can be used to retrieve a temporary list buffer from a pre-allocated cache.
    /// </summary>
    public static class ListBufferPool
    {
        private static readonly Dictionary<Type, Queue<IList>> _pool = new Dictionary<Type, Queue<IList>>();

        /// <summary>
        /// Gets a buffer from the pool. Buffers should always be returned via <see cref="ReturnBuffer"/> when no longer in use.
        /// If no buffer is available a new one will be created which is then added to the pool once <see cref="ReturnBuffer"/> is called.
        /// </summary>
        /// <typeparam name="T">The type of list</typeparam>
        /// <param name="capacityHint">A hint as to the expected capacity requirement.</param>
        /// <returns>A list from the buffer pool with room for at least <paramref name="capacityHint"/> elements.</returns>
        public static List<T> GetBuffer<T>(int capacityHint)
        {
            lock (_pool)
            {
                Queue<IList> listQueue;
                if (!_pool.TryGetValue(typeof(T), out listQueue) || listQueue.Count == 0)
                {
                    return new List<T>(capacityHint);
                }
                else
                {
                    var list = (List<T>)listQueue.Dequeue();
                    list.EnsureCapacity(capacityHint);

                    return list;
                }
            }
        }

        /// <summary>
        /// Returns a buffer to the pool.
        /// </summary>
        /// <typeparam name="T">The type of list</typeparam>
        /// <param name="buffer">The buffer.</param>
        public static void ReturnBuffer<T>(List<T> buffer)
        {
            buffer.Clear();

            lock (_pool)
            {
                Queue<IList> listQueue;
                if (!_pool.TryGetValue(typeof(T), out listQueue))
                {
                    listQueue = new Queue<IList>(1);
                    _pool[typeof(T)] = listQueue;
                }

                listQueue.Enqueue(buffer);
            }
        }

        /// <summary>
        /// Preallocates a number of list buffers.
        /// </summary>
        /// <typeparam name="T">The buffer type</typeparam>
        /// <param name="capacity">The capacity.</param>
        /// <param name="number">The number of buffers to preallocate.</param>
        public static void PreAllocate<T>(int capacity, int number = 1)
        {
            lock (_pool)
            {
                Queue<IList> listQueue;
                if (!_pool.TryGetValue(typeof(T), out listQueue))
                {
                    listQueue = new Queue<IList>(number);
                    _pool[typeof(T)] = listQueue;
                }

                for (int i = 0; i < number; i++)
                {
                    listQueue.Enqueue(new List<T>(capacity));
                }
            }
        }
    }
}
