/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Interface for types that can pre-process <see cref="IPathRequest"/>s to alter them in some way before the request is passed on to the path finder.
    /// </summary>
    public interface IRequestPreProcessor
    {
        /// <summary>
        /// Gets the priority, high number means high priority.
        /// </summary>
        int priority { get; }

        /// <summary>
        /// Pre-process the request to alter it in some way before it is passed on to the path finder.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns><c>true</c> if the request was processed; otherwise <c>false</c></returns>
        bool PreProcess(IPathRequest request);
    }
}
