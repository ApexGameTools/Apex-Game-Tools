/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.DataStructures
{
    using System;
    using System.Collections.Generic;
    using Apex.Utilities;

    /// <summary>
    /// Simple wrapper to allow lambdas for comparison for sorting where only IComparer is supported.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FunctionComparer<T> : IComparer<T>
    {
        private Comparison<T> _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionComparer{T}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public FunctionComparer(Comparison<T> comparer)
        {
            Ensure.ArgumentNotNull(comparer, "comparer");

            _comparer = comparer;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(T x, T y)
        {
            return _comparer(x, y);
        }
    }
}