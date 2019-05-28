/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;

    /// <summary>
    /// Interface that represents a Utility AI implementation.
    /// </summary>
    /// <seealso cref="Apex.AI.ISelect" />
    public interface IUtilityAI : ISelect
    {
        /// <summary>
        /// Gets or sets the name of the AI.
        /// </summary>
        /// <value>
        /// The name of the AI
        /// </value>
        string name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the root selector of the AI.
        /// </summary>
        /// <value>
        /// The root selector of the AI.
        /// </value>
        Selector rootSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the number of selectors in the AI.
        /// </summary>
        /// <value>
        /// The number of selectors in the AI.
        /// </value>
        int selectorCount
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="Selector"/> with the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Selector"/>.
        /// </value>
        /// <param name="idx">The index.</param>
        /// <returns>The selector at the specified index.</returns>
        Selector this[int idx]
        {
            get;
        }

        /// <summary>
        /// Adds the selector.
        /// </summary>
        /// <param name="s">The selector.</param>
        void AddSelector(Selector s);

        /// <summary>
        /// Removes the selector.
        /// </summary>
        /// <param name="s">The selector.</param>
        void RemoveSelector(Selector s);

        /// <summary>
        /// Replaces a selector with another selector.
        /// </summary>
        /// <param name="current">The current selector.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns><c>true</c> if the replacement was done; otherwise <c>false</c></returns>
        bool ReplaceSelector(Selector current, Selector replacement);

        /// <summary>
        /// Finds the selector with the specified ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The selector or <c>null</c> if not found.</returns>
        Selector FindSelector(Guid id);

        /// <summary>
        /// Regenerates the IDs of this AI.
        /// </summary>
        void RegenerateIds();
    }
}
