/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    /// <summary>
    /// Interface for Actions
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="context">The context.</param>
        void Execute(IAIContext context);
    }
}