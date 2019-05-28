/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;
    using Apex.Utilities;

    /// <summary>
    /// Represents an action that will be executed by a load balancer and may be repeated any number of times.
    /// </summary>
    public class RepeatableAction : ILoadBalanced
    {
        private Func<float, bool> _action;
        private int _repetitions;
        private int _repetitionCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatableAction"/> class. The action will repeat as long as <paramref name="action"/> returns <c>true</c>.
        /// </summary>
        /// <param name="action">The action to execute. The action will receive the time since it was queued as an argument, and should return whether to continue to repeat or not.</param>
        public RepeatableAction(Func<float, bool> action)
            : this(action, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatableAction"/> class.
        /// </summary>
        /// <param name="action">The action to execute. The action will receive the time since it was queued as an argument, and should return whether to continue to repeat or not.</param>
        /// <param name="repetitions">The number of times to repeat this action provided the action itself returns <c>true</c></param>
        public RepeatableAction(Func<float, bool> action, int repetitions)
        {
            Ensure.ArgumentNotNull(action, "action");

            _action = action;
            _repetitions = repetitions;
        }

        /// <summary>
        /// Gets a value indicating whether to repeatedly update this entity each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        public bool repeat
        {
            get;
            private set;
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
            this.repeat = _action(deltaTime);
            if (this.repeat && _repetitions > -1)
            {
                this.repeat = (_repetitionCount++ < _repetitions);
            }

            return null;
        }
    }
}
