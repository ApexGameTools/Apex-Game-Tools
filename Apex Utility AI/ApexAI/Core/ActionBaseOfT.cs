/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// Base class for AI actions
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Apex.AI.IAction" />
    [ApexSerializedType]
    public abstract class ActionBase<TContext> : IAction where TContext : class, IAIContext
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="context">The context.</param>
        void IAction.Execute(IAIContext context)
        {
            Execute((TContext)context);
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void Execute(TContext context);
    }
}
