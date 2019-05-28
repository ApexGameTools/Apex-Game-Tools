/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    /// <summary>
    /// Interface for enabling runtime state change on MonoBehaviours with custom editors.
    /// </summary>
    public interface ISupportRuntimeStateChange
    {
        /// <summary>
        /// Reevaluates the state.
        /// </summary>
        void ReevaluateState();
    }
}
