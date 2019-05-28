/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using UnityEngine;

    /// <summary>
    /// Possible base class for <see cref="IRequestPreProcessor"/> implementations
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="Apex.PathFinding.IRequestPreProcessor" />
    public abstract class RequestPreProcessorBase : MonoBehaviour, IRequestPreProcessor
    {
        [SerializeField]
        private int _priority = 1;

        /// <summary>
        /// Gets the priority, high number means high priority.
        /// </summary>
        public int priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// Pre-process the request to alter it in some way before it is passed on to the path finder.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>true</c> if the request was processed; otherwise <c>false</c>
        /// </returns>
        public abstract bool PreProcess(IPathRequest request);
    }
}
