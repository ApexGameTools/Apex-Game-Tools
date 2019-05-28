/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Configuration of <see cref="LoadBalancedQueue"/>s.
    /// </summary>
    [Serializable]
    public class LoadBalancerConfig
    {
        /// <summary>
        /// The update interval
        /// </summary>
        public float updateInterval = 0.1f;

        /// <summary>
        /// The maximum updates per frame
        /// </summary>
        public int maxUpdatesPerFrame = 20;

        /// <summary>
        /// The maximum update time in milliseconds per update
        /// </summary>
        public int maxUpdateTimeInMillisecondsPerUpdate = 4;

        /// <summary>
        /// Controls whether to automatically adjust <see cref="maxUpdatesPerFrame"/> and <see cref="maxUpdateTimeInMillisecondsPerUpdate"/>,
        /// such that all queued items will be evenly spread across the <see cref="updateInterval"/>.
        /// </summary>
        public bool autoAdjust;

        /// <summary>
        /// The target load balancer name
        /// </summary>
        [SerializeField, HideInInspector]
        public string targetLoadBalancer;

        /// <summary>
        /// Gets the associated load balancer.
        /// </summary>
        /// <value>
        /// The associated load balancer.
        /// </value>
        public LoadBalancedQueue associatedLoadBalancer
        {
            get;
            private set;
        }

        internal static LoadBalancerConfig From(string name, LoadBalancedQueue q)
        {
            return new LoadBalancerConfig
            {
                associatedLoadBalancer = q,
                autoAdjust = q.autoAdjust,
                maxUpdatesPerFrame = q.maxUpdatesPerInterval,
                maxUpdateTimeInMillisecondsPerUpdate = q.maxUpdateTimeInMillisecondsPerUpdate,
                updateInterval = q.defaultUpdateInterval,
                targetLoadBalancer = name
            };
        }

        internal void ApplyTo(LoadBalancedQueue q)
        {
            q.defaultUpdateInterval = this.updateInterval;
            q.maxUpdatesPerInterval = this.maxUpdatesPerFrame;
            q.maxUpdateTimeInMillisecondsPerUpdate = this.maxUpdateTimeInMillisecondsPerUpdate;
            q.autoAdjust = this.autoAdjust;

            this.associatedLoadBalancer = q;
        }
    }
}
