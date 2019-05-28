/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;

    /// <summary>
    /// Interface for load balanced items
    /// </summary>
    public interface ILoadBalanced
    {
        /// <summary>
        /// Gets a value indicating whether to repeatedly update this entity each interval.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        bool repeat { get; }

        /// <summary>
        /// Executes the update.
        /// </summary>
        /// <param name="deltaTime">The delta time, i.e. the time passed since the last update.</param>
        /// <param name="nextInterval">The time that will pass until the next update.</param>
        /// <returns>Can return the next interval by which the update should run. To use the default interval return null.</returns>
        float? ExecuteUpdate(float deltaTime, float nextInterval);
    }
}
