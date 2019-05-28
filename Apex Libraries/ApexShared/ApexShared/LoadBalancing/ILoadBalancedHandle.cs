/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;

    /// <summary>
    /// Interface to represent items added to the load balancer, which allows a number of operations to be called.
    /// </summary>
    public interface ILoadBalancedHandle
    {
        /// <summary>
        /// Gets the load balanced item to which this handle refers.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        ILoadBalanced item { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed, meaning that the <see cref="item"/> has already completed and is no longer present in the load balancer.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        bool isDisposed { get; }

        /// <summary>
        /// Stops the <see cref="item"/> from being executed in the load balancer.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the execution of <see cref="item"/> in the load balancer.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the execution of <see cref="item"/> in the load balancer.
        /// </summary>
        void Resume();
    }
}
