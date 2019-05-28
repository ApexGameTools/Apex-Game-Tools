/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    /// <summary>
    /// Exposes a <see cref="LoadBalancedQueue"/> to balance the workload of the game.
    /// Extend this class to add more load balancers.
    /// </summary>
    public class LoadBalancer
    {
        /// <summary>
        /// The default load balancer
        /// </summary>
        public static readonly ILoadBalancer defaultBalancer = new LoadBalancedQueue(4);

        /// <summary>
        /// The unscaled time load balancer
        /// </summary>
        public static readonly ILoadBalancer unscaled = new LoadBalancedQueue(4, true);

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancer"/> class. This class however is not meant to be instantiated.
        /// </summary>
        protected LoadBalancer()
        {
        }

        /// <summary>
        /// The marshaller that can be used to execute actions on the main thread from another thread
        /// </summary>
        public static IMarshaller marshaller
        {
            get;
            internal set;
        }
    }
}
