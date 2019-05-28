/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// Generic base class for AI actions that implement IRequireTermination
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Apex.AI.IAction" />
    [ApexSerializedType]
    public abstract class TerminableActionBase<TContext> : ActionBase<TContext>, IRequireTermination where TContext : class, IAIContext
    {
        /// <summary>
        /// Called when the action is no longer the active action, i.e. when another action is chosen for execution.
        /// </summary>
        /// <param name="context">The context.</param>
        void IRequireTermination.Terminate(IAIContext context)
        {
            Terminate((TContext)context);
        }

        /// <summary>
        ///  Called when the action is no longer the active action, i.e. when another action is chosen for execution.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void Terminate(TContext context);
    }
}
