/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.Common;

    /// <summary>
    /// Interface to be implemented by entities that want to issue path requests.
    /// The recommended alternative to implementing this is to use the <see cref="CallbackPathRequest"/>.
    /// </summary>
    public interface INeedPath
    {
        /// <summary>
        /// Consumes the path result.
        /// </summary>
        /// <param name="result">The result.</param>
        void ConsumePathResult(PathResult result);
    }
}
