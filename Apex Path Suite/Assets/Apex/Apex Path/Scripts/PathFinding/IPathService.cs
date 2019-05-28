/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using System.Collections;

    /// <summary>
    /// Interface for the path service
    /// </summary>
    public interface IPathService : IDisposable
    {
        /// <summary>
        /// Queues a request.
        /// </summary>
        /// <param name="request">The request.</param>
        void QueueRequest(IPathRequest request);

        /// <summary>
        /// Queues a request with a priority. Higher values take priority.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="priority">The priority.</param>
        void QueueRequest(IPathRequest request, int priority);
    }
}
