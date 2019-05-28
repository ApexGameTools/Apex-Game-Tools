/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    /// <summary>
    /// Load balancer used by the Apex Path framework
    /// </summary>
    public class NavLoadBalancer : LoadBalancer
    {
        /// <summary>
        /// The steering load balancer
        /// </summary>
        public static readonly ILoadBalancer steering = new LoadBalancedQueue(10);

        /// <summary>
        /// The dynamic obstacles  load balancer
        /// </summary>
        public static readonly ILoadBalancer dynamicObstacles = new LoadBalancedQueue(10);

        /// <summary>
        /// The scanner load balancer
        /// </summary>
        public static readonly ILoadBalancer scanners = new LoadBalancedQueue(10, 1f, 20, 2);

        /// <summary>
        /// Prevents a default instance of the <see cref="NavLoadBalancer"/> class from being created.
        /// </summary>
        private NavLoadBalancer()
        {
        }
    }
}
