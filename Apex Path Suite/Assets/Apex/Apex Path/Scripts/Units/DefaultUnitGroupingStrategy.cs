/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using System.Collections;

    /// <summary>
    /// The default grouping strategy. This will simply create one group for all units.
    /// </summary>
    public class DefaultUnitGroupingStrategy : IGroupingStrategy<IUnitFacade>
    {
        /// <summary>
        /// Creates the grouping with members.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <returns>
        /// The grouping
        /// </returns>
        public IGrouping<IUnitFacade> CreateGrouping(IEnumerable members)
        {
            return new DefaultTransientUnitGroup(members.ToUnitFacades());
        }

        /// <summary>
        /// Creates an empty group with pre-allocated memory.
        /// </summary>
        /// <param name="capacity">The pre-allocation capacity.</param>
        /// <returns>
        /// The group
        /// </returns>
        public TransientGroup<IUnitFacade> CreateGroup(int capacity)
        {
            return new DefaultTransientUnitGroup(capacity);
        }

        /// <summary>
        /// Evaluates if two unit should belong to the same group
        /// </summary>
        /// <param name="lhs">The first unit.</param>
        /// <param name="rhs">The second unit.</param>
        /// <returns><c>true</c> if the two units should belong to the same group; otherwise <c>false</c></returns>
        public bool BelongsToSameGroup(IUnitFacade lhs, IUnitFacade rhs)
        {
            return true;
        }
    }
}