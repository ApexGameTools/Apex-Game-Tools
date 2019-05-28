/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using Apex.PathFinding;

    /// <summary>
    /// Interface for <see cref="PathResult"/> processors.
    /// </summary>
    public interface IProcessPathResults
    {
        /// <summary>
        /// Gets the processing order in relation to other processors
        /// </summary>
        /// <value>
        /// The processing order.
        /// </value>
        int processingOrder { get; }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="steerer">The steerer.</param>
        /// <returns><c>true</c> if the result was handled by this processor, otherwise <c>false</c></returns>
        bool HandleResult(PathResult result, SteerForPathComponent steerer);
    }
}
