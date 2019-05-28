/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Manages grouping strategies and exposes a number of utility methods for group creation.
    /// </summary>
    public static class GroupingManager
    {
        private static readonly IDictionary<Type, object> _groupingStrategies = new Dictionary<Type, object>();

        /// <summary>
        /// Registers the grouping strategy.
        /// </summary>
        /// <typeparam name="T">The target unit type of the strategy</typeparam>
        /// <param name="strat">The strategy.</param>
        public static void RegisterGroupingStrategy<T>(IGroupingStrategy<T> strat) where T : IGroupable<T>
        {
            _groupingStrategies[typeof(T)] = strat;
        }

        /// <summary>
        /// Gets the grouping strategy.
        /// </summary>
        /// <typeparam name="T">The target unit type of the strategy</typeparam>
        /// <returns>The grouping strategy for the specified unit type</returns>
        public static IGroupingStrategy<T> GetGroupingStrategy<T>() where T : IGroupable<T>
        {
            object tmp;
            if (_groupingStrategies.TryGetValue(typeof(T), out tmp))
            {
                return (IGroupingStrategy<T>)tmp;
            }

            return null;
        }

        /// <summary>
        /// Creates a grouping based on the current grouping strategy.
        /// </summary>
        /// <typeparam name="T">The unit type</typeparam>
        /// <param name="members">The members.</param>
        /// <returns>A grouping</returns>
        public static IGrouping<T> CreateGrouping<T>(params T[] members) where T : IGroupable<T>
        {
            return CreateGrouping((IEnumerable<T>)members);
        }

        /// <summary>
        /// Creates a grouping based on the current grouping strategy.
        /// </summary>
        /// <typeparam name="T">The unit type</typeparam>
        /// <param name="members">The members.</param>
        /// <returns>A grouping</returns>
        public static IGrouping<T> CreateGrouping<T>(IEnumerable<T> members) where T : IGroupable<T>
        {
            var strat = GetGroupingStrategy<T>();
            if (strat == null)
            {
                return null;
            }

            return strat.CreateGrouping(members);
        }

        /// <summary>
        /// Creates an empty group for the specified unit type.
        /// </summary>
        /// <typeparam name="T">The unit type</typeparam>
        /// <param name="capacity">The capacity.</param>
        /// <returns>An empty group</returns>
        public static TransientGroup<T> CreateGroup<T>(int capacity) where T : IGroupable<T>
        {
            var strat = GetGroupingStrategy<T>();
            if (strat == null)
            {
                return null;
            }

            return strat.CreateGroup(capacity);
        }

        /// <summary>
        /// Creates a group with the specified members.
        /// </summary>
        /// <typeparam name="T">he unit type</typeparam>
        /// <param name="members">The members.</param>
        /// <returns>The group</returns>
        public static TransientGroup<T> CreateGroup<T>(IEnumerable<T> members) where T : IGroupable<T>
        {
            var strat = GetGroupingStrategy<T>();
            if (strat == null)
            {
                return null;
            }

            //Enumerate it once
            var memberList = members.ToList();
            var grp = strat.CreateGroup(memberList.Count);
            foreach (var m in memberList)
            {
                grp.Add(m);
            }

            return grp;
        }
    }
}
