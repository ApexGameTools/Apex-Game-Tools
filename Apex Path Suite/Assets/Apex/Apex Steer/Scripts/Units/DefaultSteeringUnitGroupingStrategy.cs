/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// The default <see cref="IGroupingStrategy{T}"/> for Apex Steer.
    /// Uses IUnitFacade to represent units, instead <see cref="DefaultTransientUnitGroup"/> if grouping only includes one unit, otherwise <see cref="DefaultSteeringTransientUnitGroup"/> for groups.
    /// </summary>
    public class DefaultSteeringUnitGroupingStrategy : IGroupingStrategy<IUnitFacade>
    {
        /// <summary>
        /// Creates the grouping with members.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <returns>The grouping</returns>
        public IGrouping<IUnitFacade> CreateGrouping(IEnumerable members)
        {
            // convert list to a list of unit facades
            var units = members.ToUnitFacades();

            if (units.Count() == 1)
            {
                return new DefaultTransientUnitGroup(units);
            }

            return new DefaultSteeringTransientUnitGroup(units);
        }

        /// <summary>
        /// Creates an empty group with pre-allocated memory.
        /// </summary>
        /// <param name="capacity">The pre-allocation capacity.</param>
        /// <returns>The group</returns>
        public TransientGroup<IUnitFacade> CreateGroup(int capacity)
        {
            return new DefaultSteeringTransientUnitGroup(capacity);
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