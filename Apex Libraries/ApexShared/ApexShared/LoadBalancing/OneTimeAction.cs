/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;
    using Apex.Utilities;

    /// <summary>
    /// Represents a one-off action that will be executed by a load balancer.
    /// </summary>
    public class OneTimeAction : ILoadBalanced
    {
        private Action<float> _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneTimeAction"/> class.
        /// </summary>
        /// <param name="action">The action to execute. The action will receive the time since it was queued as an argument.</param>
        public OneTimeAction(Action<float> action)
        {
            Ensure.ArgumentNotNull(action, "action");

            _action = action;
        }

        /// <summary>
        /// Gets a value indicating whether to repeatedly update this entity each interval. This will always be <c>false</c> for a <see cref="OneTimeAction"/>
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        public bool repeat
        {
            get { return false; }
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
            _action(deltaTime);
            return null;
        }
    }
}
