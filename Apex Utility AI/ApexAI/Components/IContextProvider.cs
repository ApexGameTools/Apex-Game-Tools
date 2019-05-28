/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Components
{
    using System;

    /// <summary>
    /// Interface for context provider responsible for supplying the <see cref="IAIContext"/> implementing context instances for e.g. AI clients.
    /// </summary>
    public interface IContextProvider
    {
        /// <summary>
        /// Retrieves the context instance. This can be a simple getter or a factory method.
        /// </summary>
        /// <param name="aiId">The Id of the requesting AI.</param>
        /// <returns>The concrete context instance for use by the requester.</returns>
        IAIContext GetContext(Guid aiId);
    }
}
