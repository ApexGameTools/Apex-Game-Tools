/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    using System;
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.Utilities;

    /// <summary>
    /// Represents a grouping of units. A grouping is defined as a group of groups of units.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Grouping<T> : IGrouping<T> where T : IGroupable<T>
    {
        private DynamicArray<TransientGroup<T>> _members;

        /// <summary>
        /// Initializes a new instance of the <see cref="Grouping{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public Grouping(int capacity)
        {
            _members = new DynamicArray<TransientGroup<T>>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grouping{T}"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public Grouping(IEnumerable<TransientGroup<T>> members)
        {
            Ensure.ArgumentNotNull(members, "members");

            _members = new DynamicArray<TransientGroup<T>>(members);
        }

        /// <summary>
        /// Gets number of groups in this grouping.
        /// </summary>
        /// <value>
        /// The group count.
        /// </value>
        public int groupCount
        {
            get { return _members.count; }
        }

        /// <summary>
        /// Gets number of members in this grouping, i.e. total members across all groups.
        /// </summary>
        /// <value>
        /// The member count.
        /// </value>
        public int memberCount
        {
            get
            {
                int count = 0;
                var grpCount = _members.count;
                for (int i = 0; i < grpCount; i++)
                {
                    count += _members[i].count;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the <see cref="TransientGroup{T}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="TransientGroup{T}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public TransientGroup<T> this[int index]
        {
            get { return _members[index]; }
        }

        /// <summary>
        /// Adds the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public virtual void Add(TransientGroup<T> group)
        {
            _members.Add(group);
        }

        /// <summary>
        /// Removes the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public virtual void Remove(TransientGroup<T> group)
        {
            _members.Remove(group);
        }

        /// <summary>
        /// Adds the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <exception cref="System.InvalidOperationException">No strategy exists for this type of member.</exception>
        public void Add(T member)
        {
            var strat = GroupingManager.GetGroupingStrategy<T>();
            if (strat == null)
            {
                throw new InvalidOperationException("No strategy exists for this type of member.");
            }

            var grpCount = _members.count;
            for (int i = 0; i < grpCount; i++)
            {
                var grp = _members[i];
                if (grp.count == 0)
                {
                    continue;
                }

                if (strat.BelongsToSameGroup(grp[0], member))
                {
                    grp.Add(member);
                    return;
                }
            }

            var newGrp = strat.CreateGroup(1);
            newGrp.Add(member);
            this.Add(newGrp);
        }

        /// <summary>
        /// Removes the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <exception cref="System.InvalidOperationException">No strategy exists for this type of member.</exception>
        public void Remove(T member)
        {
            var strat = GroupingManager.GetGroupingStrategy<T>();
            if (strat == null)
            {
                throw new InvalidOperationException("No strategy exists for this type of member.");
            }

            var grpCount = _members.count;
            for (int i = 0; i < grpCount; i++)
            {
                var grp = _members[i];
                if (grp.count == 0)
                {
                    continue;
                }

                if (strat.BelongsToSameGroup(grp[0], member))
                {
                    grp.Remove(member);
                    return;
                }
            }
        }
    }
}