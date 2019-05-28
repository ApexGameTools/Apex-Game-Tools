/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using System.Collections;

    /// <summary>
    /// Interface for grouping strategies
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGroupingStrategy<T> where T : IGroupable<T>
    {
        /// <summary>
        /// Creates the grouping with members.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <returns>The grouping</returns>
        IGrouping<T> CreateGrouping(IEnumerable members);

        /// <summary>
        /// Creates an empty group with pre-allocated memory.
        /// </summary>
        /// <param name="capacity">The pre-allocation capacity.</param>
        /// <returns>The group</returns>
        TransientGroup<T> CreateGroup(int capacity);

        /// <summary>
        /// Evaluates if two objects should belong to the same group
        /// </summary>
        /// <param name="lhs">The first object.</param>
        /// <param name="rhs">The second object.</param>
        /// <returns><c>true</c> if the two objects should belong to the same group; otherwise <c>false</c></returns>
        bool BelongsToSameGroup(T lhs, T rhs);
    }
}