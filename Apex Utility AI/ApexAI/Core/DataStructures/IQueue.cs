/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    /// <summary>
    /// Interface for a typed Queue
    /// </summary>
    /// <typeparam name="T">The type stored.</typeparam>
    public interface IQueue<T>
    {
        /// <summary>
        /// Enqueues the specified object.
        /// </summary>
        /// <param name="obj">The object to add to the queue.</param>
        void Enqueue(T obj);

        /// <summary>
        /// Removes the first item in the queue and returns it.
        /// </summary>
        /// <returns>The first item in the queue.</returns>
        T Dequeue();
    }
}
