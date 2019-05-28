/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    /// <summary>
    /// Interface for groupings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGrouping<T> where T : IGroupable<T> 
    {
        /// <summary>
        /// Gets number of groups in this grouping.
        /// </summary>
        /// <value>
        /// The group count.
        /// </value>
        int groupCount
        {
            get;
        }

        /// <summary>
        /// Gets number of members in this grouping, i.e. total members across all groups.
        /// </summary>
        /// <value>
        /// The member count.
        /// </value>
        int memberCount
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="TransientGroup{T}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="TransientGroup{T}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        TransientGroup<T> this[int index]
        {
            get;
        }

        /// <summary>
        /// Adds the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        void Add(T member);

        /// <summary>
        /// Removes the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        void Remove(T member);
    }
}
