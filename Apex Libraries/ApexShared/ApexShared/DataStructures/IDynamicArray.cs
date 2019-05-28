/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    /// <summary>
    /// Interface for Dynamic Arrays
    /// </summary>
    /// <typeparam name="T">The array item type.</typeparam>
    /// <seealso cref="Apex.DataStructures.IIterable{T}" />
    public interface IDynamicArray<T> : IIterable<T>
    {
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        void Add(T item);

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if found and removed; otherwise <c>false</c></returns>
        bool Remove(T item);

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Clears the array.
        /// </summary>
        void Clear();

        /// <summary>
        /// Ensures a certain capacity of the array, i.e. resizes the array to hold the specified number of items if not already able to.
        /// </summary>
        /// <param name="capacity">The capacity to ensure.</param>
        void EnsureCapacity(int capacity);
    }
}
