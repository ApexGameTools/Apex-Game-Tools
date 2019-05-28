/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using Apex.PathFinding;
    using UnityEngine;

    /// <summary>
    /// Base class for components that process pathing results in the <see cref="SteerForPathComponent"/> pipeline.
    /// </summary>
    [RequireComponent(typeof(SteerForPathComponent))]
    public abstract class SteerForPathResultProcessorComponent : MonoBehaviour, IProcessPathResults
    {
        /// <summary>
        /// The processing order
        /// </summary>
        public int processingOrder;

        int IProcessPathResults.processingOrder
        {
            get { return this.processingOrder; }
        }

        private void Awake()
        {
            this.WarnIfMultipleInstances();
        }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="steerer">The steerer.</param>
        /// <returns>
        ///   <c>true</c> if the result was handled by this processor, otherwise <c>false</c>
        /// </returns>
        public abstract bool HandleResult(PathResult result, SteerForPathComponent steerer);
    }
}
