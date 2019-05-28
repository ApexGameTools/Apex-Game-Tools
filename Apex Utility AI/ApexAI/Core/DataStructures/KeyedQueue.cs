/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex.Utilities;

    /// <summary>
    /// A combination of a queue and a hashset. Lookups are O(1) and the data structure is indexable.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    public class KeyedQueue<T, TKey> : IIterable<T>, IQueue<T>
    {
        private HashSet<TKey> _hashset;
        private SimpleQueue<T> _queue;
        private Func<T, TKey> _keyProvider;
        private bool _strictSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedQueue{T, TKey}"/> class.
        /// </summary>
        /// <param name="keyProvider">The function to select the key to use for the object stored in the queue.</param>
        /// <param name="strictSet">If <see langword="true"/> the queue will only accept a given object once, even if the object is no longer queued the same object cannot be added again.</param>
        public KeyedQueue(Func<T, TKey> keyProvider, bool strictSet)
            : this(0, keyProvider, strictSet)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedQueue{T, TKey}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="keyProvider">The function to select the key to use for the object stored in the queue.</param>
        /// <param name="strictSet">If <see langword="true"/> the queue will only accept a given object once, even if the object is no longer queued the same object cannot be added again.</param>
        public KeyedQueue(int capacity, Func<T, TKey> keyProvider, bool strictSet)
        {
            Ensure.ArgumentInRange(() => capacity >= 0, "capacity", capacity);

            _hashset = new HashSet<TKey>();
            _queue = new SimpleQueue<T>(capacity);
            _keyProvider = keyProvider;
            _strictSet = strictSet;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int count
        {
            get { return _queue.count; }
        }

        /// <summary>
        /// Gets the value with the specified index.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <returns>The value at the index</returns>
        public T this[int idx]
        {
            get { return _queue[idx]; }
        }

        /// <summary>
        /// Enqueues the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Enqueue(T obj)
        {
            if (_hashset.Add(_keyProvider(obj)))
            {
                _queue.Enqueue(obj);
            }
        }

        /// <summary>
        /// Removes the first item in the queue and returns it.
        /// </summary>
        /// <returns>
        /// The first item in the queue.
        /// </returns>
        public T Dequeue()
        {
            T obj = _queue.Dequeue();

            if (!_strictSet)
            {
                _hashset.Remove(_keyProvider(obj));
            }

            return obj;
        }

        /// <summary>
        /// Determines whether the set contains the object, or once did if the queue was created as a strict set.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if the object is (or was) contained in the set; otherwise <c>false</c></returns>
        public bool Contains(T obj)
        {
            return _hashset.Contains(_keyProvider(obj));
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
            _hashset.Clear();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                yield return _queue[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                yield return _queue[i];
            }
        }
    }
}