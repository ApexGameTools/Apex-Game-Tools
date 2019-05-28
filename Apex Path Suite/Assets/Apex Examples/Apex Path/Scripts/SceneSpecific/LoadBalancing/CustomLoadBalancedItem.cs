/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using Apex.LoadBalancing;

    /// <summary>
    /// A custom load balanced item.
    /// </summary>
    public class CustomLoadBalancedItem : ILoadBalanced
    {
        /// <summary>
        /// Gets or set a value indicating whether to repeatedly update this entity each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        public bool repeat
        {
            get;
            set;
        }

        /// <summary>
        /// Executes the update.
        /// </summary>
        /// <param name="deltaTime">The delta time, i.e. the time passed since the last update.</param>
        /// <param name="nextInterval">The time that will pass until the next update.</param>
        /// <returns>
        /// Can return the next interval by which the update should run. To use the default interval return null.
        /// </returns>
        public float? ExecuteUpdate(float deltaTime, float nextInterval)
        {
            //Do the work you need to do here
            return null;
        }
    }
}
