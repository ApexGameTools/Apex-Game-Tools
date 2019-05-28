/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    /// <summary>
    /// Interface for groupable unit
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGroupable<T> where T : IGroupable<T>
    {
        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The transient group.
        /// </value>
        TransientGroup<T> transientGroup { get; set; }

        /// <summary>
        /// Gets or sets the formation index, i.e. the place in the group formation.
        /// </summary>
        /// <value>
        /// The index of the formation.
        /// </value>
        int formationIndex { get; set; }
    }
}
