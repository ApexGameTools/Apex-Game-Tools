/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// The types that an <see cref="IPathRequest"/> can be.
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// A normal request
        /// </summary>
        Normal,

        /// <summary>
        /// A request to only get information back to indicate the cost and availability of a path, but not the actual path.
        /// Use this for AI requests where the path itself is not going to be used.
        /// </summary>
        IntelOnly
    }
}
