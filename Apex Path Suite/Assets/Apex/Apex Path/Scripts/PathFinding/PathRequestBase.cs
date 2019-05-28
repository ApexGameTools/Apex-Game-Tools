/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Base class for path request types.
    /// </summary>
    public abstract class PathRequestBase
    {
        /// <summary>
        /// Gets or sets where to move from.
        /// </summary>
        public Vector3 from
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets where to move to.
        /// </summary>
        public Vector3 to
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the points in between <see cref="from"/> and <see cref="to"/> that the path should include.
        /// </summary>
        public Vector3[] via
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the requester of this path, i.e. the entity that needs a path.
        /// </summary>
        public virtual INeedPath requester
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the requester's properties.
        /// </summary>
        public IUnitProperties requesterProperties
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type of this request.
        /// </summary>
        public RequestType type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path finder options.
        /// </summary>
        /// <value>
        /// The path finder options.
        /// </value>
        public IPathFinderOptions pathFinderOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp when this request was made.
        /// </summary>
        public float timeStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets custom data, which can be used by e.g. pre and post processors to communicate.
        /// </summary>
        public object customData
        {
            get;
            set;
        }
    }
}
