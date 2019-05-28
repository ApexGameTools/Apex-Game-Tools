/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Exposes various common extensions
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Returns the item scoring the highest in a given list
        /// </summary>
        /// <typeparam name="T">The type of the item to score</typeparam>
        /// <typeparam name="TResult">The score type</typeparam>
        /// <param name="list">The list of items</param>
        /// <param name="scorer">The scoring function</param>
        /// <returns>The item attaining the highest score</returns>
        public static T MaxScored<T, TResult>(this IEnumerable<T> list, Func<T, TResult> scorer) where TResult : IComparable<TResult>
        {
            TResult maxScore = default(TResult);
            T result = default(T);

            foreach (var item in list)
            {
                var score = scorer(item);

                if (score.CompareTo(maxScore) > 0)
                {
                    maxScore = score;
                    result = item;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified entity has any of the specified attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="attributes">The attributes to check for.</param>
        /// <returns><c>true</c> if the entity has at least one of the specified attributes, otherwise, <c>false</c></returns>
        public static bool HasAny(this IHaveAttributes entity, AttributeMask attributes)
        {
            return (entity.attributes & attributes) > 0;
        }

        /// <summary>
        /// Determines whether the specified entity has all of the specified attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="attributes">The attributes to check for.</param>
        /// <returns><c>true</c> if the entity has all of the specified attributes, otherwise, <c>false</c></returns>
        public static bool HasAll(this IHaveAttributes entity, AttributeMask attributes)
        {
            return (entity.attributes & attributes) == attributes;
        }

        /// <summary>
        /// Iterates over all members of all groups in the specified grouping.
        /// </summary>
        /// <typeparam name="T">The type grouped</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <returns>An enumerator</returns>
        public static IEnumerable<T> All<T>(this IGrouping<T> grouping) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                var grp = grouping[i];
                var memberCount = grp.count;
                for (int j = 0; j < memberCount; j++)
                {
                    yield return grp[j];
                }
            }
        }

        /// <summary>
        /// Sets the preferred speed on a grouping.
        /// </summary>
        /// <typeparam name="T">The type grouped</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="speed">The speed.</param>
        public static void SetPreferredSpeed<T>(this IGrouping<T> grouping, float speed) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].SetPreferredSpeed(speed);
            }
        }

        /// <summary>
        /// Stops the specified grouping.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        public static void Stop<T>(this IGrouping<T> grouping) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].Stop();
            }
        }

        /// <summary>
        /// Makes the grouping wait.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="seconds">The seconds to wait or null to wait until explicitly <see cref="Resume"/>d.</param>
        public static void Wait<T>(this IGrouping<T> grouping, float? seconds) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].Wait(seconds);
            }
        }

        /// <summary>
        /// Resumes the specified grouping's movement.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        public static void Resume<T>(this IGrouping<T> grouping) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].Resume();
            }
        }

        /// <summary>
        /// Asks the grouping to move to the specified position
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="position">The position to move to.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        public static void MoveTo<T>(this IGrouping<T> grouping, Vector3 position, bool append) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].MoveTo(position, append);
            }
        }

        /// <summary>
        /// Asks the grouping to move along the specified path. Replanning is done by the path finder.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="path">The path.</param>
        public static void MoveAlong<T>(this IGrouping<T> grouping, Path path) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].MoveAlong(path);
            }
        }

        /// <summary>
        /// Asks the grouping to move along the specified path.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        public static void MoveAlong<T>(this IGrouping<T> grouping, Path path, ReplanCallback onReplan) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].MoveAlong(path, onReplan);
            }
        }

        /// <summary>
        /// Enables movement orders for the grouping.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        public static void EnableMovementOrders<T>(this IGrouping<T> grouping) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].EnableMovementOrders();
            }
        }

        /// <summary>
        /// Disables movement orders for the grouping.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        public static void DisableMovementOrders<T>(this IGrouping<T> grouping) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].DisableMovementOrders();
            }
        }

        /// <summary>
        /// Adds members to the grouping.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="members">The members.</param>
        public static void Add<T>(this IGrouping<T> grouping, params T[] members) where T : IGroupable<T>
        {
            var count = members.Length;
            for (int i = 0; i < count; i++)
            {
                grouping.Add(members[i]);
            }
        }

        /// <summary>
        /// Adds members to the grouping.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="members">The members.</param>
        public static void Add<T>(this IGrouping<T> grouping, IEnumerable<T> members) where T : IGroupable<T>
        {
            foreach (var member in members)
            {
                grouping.Add(member);
            }
        }

        /// <summary>
        /// Removes members from the grouping.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="members">The members.</param>
        public static void Remove<T>(this IGrouping<T> grouping, params T[] members) where T : IGroupable<T>
        {
            var count = members.Length;
            for (int i = 0; i < count; i++)
            {
                grouping.Remove(members[i]);
            }
        }

        /// <summary>
        /// Removes members from the grouping.
        /// </summary>
        /// <typeparam name="T">Type of members in the grouping</typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="members">The members.</param>
        public static void Remove<T>(this IGrouping<T> grouping, IEnumerable<T> members) where T : IGroupable<T>
        {
            foreach (var member in members)
            {
                grouping.Remove(member);
            }
        }

        /// <summary>
        /// Creates a path request for the unit.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="to">The destination.</param>
        /// <param name="callback">TThe callback to be called when the result is ready.</param>
        /// <returns>A path request for the unit.</returns>
        public static IPathRequest CreatePathRequest(this IUnitFacade unit, Vector3 to, Action<PathResult> callback)
        {
            return new CallbackPathRequest(callback)
            {
                from = unit.position,
                to = to,
                requesterProperties = unit,
                pathFinderOptions = unit.pathFinderOptions
            };
        }

        /// <summary>
        /// Converts a list of objects to a list of unit facades if applicable. The resulting list will only contain elements that could be resolved as IUnitFacades.
        /// </summary>
        /// <param name="list">The list to resolve from.</param>
        /// <returns>A list of IUnitFacades.</returns>
        public static IEnumerable<IUnitFacade> ToUnitFacades(this IEnumerable list)
        {
            var units = from m in list.Cast<object>()
                        let unit = ResolveUnit(m)
                        where unit != null
                        select unit;

            return units;
        }

        private static IUnitFacade ResolveUnit(object member)
        {
            var unit = member as IUnitFacade;
            if (unit != null)
            {
                return unit;
            }

            var goComp = member as IGameObjectComponent;
            if (goComp != null)
            {
                return goComp.GetUnitFacade();
            }

            var comp = member as Component;
            if (comp != null)
            {
                return comp.GetUnitFacade();
            }

            var go = member as GameObject;
            if (go != null)
            {
                return go.GetUnitFacade();
            }

            return null;
        }
    }
}
