/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Interface to be implemented by <see cref="IAction"/>s that need to be told when they are no longer the active action. See Remarks.
    /// </summary>
    /// <remarks>
    /// An action that implements this interface should not be added to a <see cref="CompositeAction"/> as it will not be respected, instead <see cref="TerminableCompositeAction"/> can be used.
    /// </remarks>
    public interface IRequireTermination
    {
        /// <summary>
        /// Called when the action is no longer the active action, i.e. when another action is chosen for execution.
        /// </summary>
        /// <param name="context">The context.</param>
        void Terminate(IAIContext context);
    }
}
