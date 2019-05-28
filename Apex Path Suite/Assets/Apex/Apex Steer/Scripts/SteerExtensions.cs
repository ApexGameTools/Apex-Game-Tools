/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex
{
    using Apex.Steering;
    using Apex.Units;

    /// <summary>
    /// Exposes various common extensions
    /// </summary>
    public static class SteerExtensions
    {
        /// <summary>
        /// Sets a formation for all groups in a grouping.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="formation">The formation.</param>
        public static void SetFormation<T>(this IGrouping<T> grouping, IFormation formation) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].SetFormation(formation);
            }
        }

        /// <summary>
        /// Clears the formation for all groups in a grouping.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="grouping">The grouping.</param>
        /// <param name="formation">The formation.</param>
        public static void ClearFormation<T>(this IGrouping<T> grouping) where T : IGroupable<T>
        {
            var groupCount = grouping.groupCount;
            for (int i = 0; i < groupCount; i++)
            {
                grouping[i].ClearFormation();
            }
        }
    }
}