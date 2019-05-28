/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using System.Collections;

    /// <summary>
    /// Interface for pathing engines
    /// </summary>
    public interface IPathingEngine
    {
        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        void ProcessRequest(IPathRequest request);

        /// <summary>
        /// Processes the request as a coroutine.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The coroutine enumerator</returns>
        IEnumerator ProcessRequestCoroutine(IPathRequest request);
    }
}
