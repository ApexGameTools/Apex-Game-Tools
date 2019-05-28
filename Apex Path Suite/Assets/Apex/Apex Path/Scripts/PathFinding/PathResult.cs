/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.DataStructures;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// The result of a <see cref="IPathRequest"/>
    /// </summary>
    public class PathResult
    {
        private static readonly Path _pathEmpty = new Path();
        private PathingStatus _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="path">The path.</param>
        /// <param name="pathCost">The cost of the path, i.e. its length and combined cost of the cells involved</param>
        /// <param name="originalRequest">The original request.</param>
        public PathResult(PathingStatus status, Path path, int pathCost, IPathRequest originalRequest)
        {
            this.status = status;
            this.path = path ?? _pathEmpty;
            this.pathCost = pathCost;
            this.originalRequest = originalRequest;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathResult"/> class.
        /// </summary>
        protected PathResult()
        {
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public PathingStatus status
        {
            get { return _status; }
            set { _status = value; }
        }

        /// <summary>
        /// Gets or sets the error information. Consult this if the path request fails for unknown reasons.
        /// </summary>
        /// <value>
        /// The error information.
        /// </value>
        public string errorInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public Path path
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path cost. The cost is a number that represents the length of the path combined with the cost of the nodes along it.
        /// </summary>
        /// <value>
        /// The path cost.
        /// </value>
        public int pathCost
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the original request.
        /// </summary>
        /// <value>
        /// The original request.
        /// </value>
        public IPathRequest originalRequest
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the inner result. This provides additional info on the result of the request in case of a partial success, e.g. status = <see cref="PathingStatus.CompletePartial"/>.
        /// In other cases this will be null.
        /// </summary>
        public InnerResultData innerResult
        {
            get;
            private set;
        }

        internal void RegisterPartialResult(PathingStatus status, Vector3[] pendingWaypoints)
        {
            this.status = PathingStatus.CompletePartial;

            this.innerResult = new InnerResultData
            {
                status = status,
                pendingWaypoints = pendingWaypoints
            };
        }

        /// <summary>
        /// Inner data class, nothing to see here.
        /// </summary>
        public class InnerResultData
        {
            /// <summary>
            /// The status of the segment that could not be completed.
            /// </summary>
            public PathingStatus status;

            /// <summary>
            /// The pending waypoints, e.g. the way points (or via points) that are not part of the returned path.
            /// </summary>
            public Vector3[] pendingWaypoints;

            /// <summary>
            /// Gets a value indicating whether this instance has pending waypoints.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance has pending waypoints; otherwise, <c>false</c>.
            /// </value>
            public bool hasPendingWaypoints
            {
                get { return this.pendingWaypoints != null && this.pendingWaypoints.Length > 0; }
            }
        }
    }
}
