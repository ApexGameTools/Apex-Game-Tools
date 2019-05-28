/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// Base class for AI actions
    /// </summary>
    /// <seealso cref="Apex.AI.IAction" />
    [ApexSerializedType]
    public abstract class ActionBase : IAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void Execute(IAIContext context);
    }
}
