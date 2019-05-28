/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using Apex.LoadBalancing;

    /// <summary>
    /// Load Balancer for AIs
    /// </summary>
    /// <seealso cref="Apex.LoadBalancing.LoadBalancer" />
    public sealed class AILoadBalancer : LoadBalancer
    {
        /// <summary>
        /// The AI load balancer
        /// </summary>
        public static readonly ILoadBalancer aiLoadBalancer = new LoadBalancedQueue(20, 1f, 200, 4);

        /// <summary>
        /// Prevents a default instance of the <see cref="AILoadBalancer"/> class from being created.
        /// </summary>
        private AILoadBalancer()
        {
        }
    }
}
