/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.DataStructures;
    using Apex.WorldGeometry;

    /// <summary>
    /// The standard path request to use for most pathing scenarios.
    /// </summary>
    public class BasicPathRequest : PathRequestBase, IPathRequest
    {
        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool isValid
        {
            get { return ((this.requester != null) && (this.requesterProperties != null) && (this.pathFinderOptions != null)); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has decayed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has decayed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool hasDecayed
        {
            get;
            set;
        }

        /// <summary>
        /// Completes this request
        /// </summary>
        /// <param name="result">The result.</param>
        void IPathRequest.Complete(PathResult result)
        {
            this.requester.ConsumePathResult(result);
        }
    }
}
