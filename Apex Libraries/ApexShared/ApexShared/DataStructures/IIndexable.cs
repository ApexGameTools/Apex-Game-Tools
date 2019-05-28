/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.DataStructures
{
    /// <summary>
    /// Interface for indexable ADTs
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    public interface IIndexable<T>
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int count { get; }

        /// <summary>
        /// Gets the value with the specified index.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <returns>The value at the index</returns>
        T this[int idx] { get; }
    }
}