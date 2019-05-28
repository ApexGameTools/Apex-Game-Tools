/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System.Collections.Generic;

    /// <summary>
    /// Common interface for sortable types, i.e. types that have members that can be sorted.
    /// </summary>
    /// <typeparam name="T">Type of the members</typeparam>
    public interface ISortable<T>
    {
        /// <summary>
        /// Sorts this instance using the default comparer of its members.
        /// </summary>
        void Sort();

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        void Sort(IComparer<T> comparer);

        /// <summary>
        /// Sorts a subset of this instance using the default comparer of its members.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        void Sort(int index, int length);

        /// <summary>
        /// Sorts a subset of this instance using the specified comparer.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="length">The length.</param>
        /// <param name="comparer">The comparer.</param>
        void Sort(int index, int length, IComparer<T> comparer);
    }
}
