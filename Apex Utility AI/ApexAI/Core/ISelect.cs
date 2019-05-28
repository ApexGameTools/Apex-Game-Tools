/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System;

    /// <summary>
    /// Interface for entities that select an <see cref="IAction"/> to execute.
    /// </summary>
    public interface ISelect
    {
        /// <summary>
        /// Gets the identifier of the entity.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        Guid id { get; }

        /// <summary>
        /// Selects the action for execution, given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The action to execute.</returns>
        IAction Select(IAIContext context);
    }
}