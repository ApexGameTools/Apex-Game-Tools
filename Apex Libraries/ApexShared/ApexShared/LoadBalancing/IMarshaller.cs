/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;

    /// <summary>
    /// Interface for marshalling actions from a thread onto the main thread.
    /// </summary>
    public interface IMarshaller
    {
        /// <summary>
        /// Executes the action on the main thread.
        /// </summary>
        /// <param name="a">The action.</param>
        void ExecuteOnMainThread(Action a);
    }
}
