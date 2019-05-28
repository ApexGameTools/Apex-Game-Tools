/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using Apex.LoadBalancing;
    using Apex.PathFinding;
    using Apex.Services;
    using UnityEngine;

    /// <summary>
    /// Processes path results and tries to replan the same route if the status of the result is <see cref="Apex.PathFinding.PathingStatus.NoRouteExists"/>.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Path Post Processing/Replan when no Route", 1020)]
    [ApexComponent("Behaviours")]
    public class SteerForPathReplanWhenNoRoute : SteerForPathResultProcessorComponent
    {
        /// <summary>
        /// The retry delay
        /// </summary>
        public float retryDelay = 0.1f;

        /// <summary>
        /// The maximum retries
        /// </summary>
        public int maxRetries = 3;

        private int _retries;

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="steerer">The steerer.</param>
        /// <returns>
        ///   <c>true</c> if the result was handled by this processor, otherwise <c>false</c>
        /// </returns>
        public override bool HandleResult(PathResult result, SteerForPathComponent steerer)
        {
            switch (result.status)
            {
                case PathingStatus.NoRouteExists:
                {
                    var request = result.originalRequest;

                    if (_retries < this.maxRetries)
                    {
                        _retries++;

                        //Having this as a separate call apparently avoids allocation of anonymous method, which otherwise happens even if the status is not the one triggering this action.
                        IssueRequest(request, steerer);
                        return true;
                    }

                    break;
                }
            }

            _retries = 0;
            return false;
        }

        private void IssueRequest(IPathRequest request, SteerForPathComponent steerer)
        {
            var action = new OneTimeAction((ignored) =>
            {
                steerer.RequestPath(request);
            });

            NavLoadBalancer.defaultBalancer.Add(action, this.retryDelay, true);
        }
    }
}
