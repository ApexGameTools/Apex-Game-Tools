/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using Apex.Utilities;

    /// <summary>
    /// Represents an action that can run over several frames. This type of action will run only once, but it will execute over as many frames as needed.
    /// </summary>
    public class LongRunningAction : ILoadBalanced
    {
        private Func<IEnumerator> _action;
        private Action _callback;
        private int _maxMillisecondUsedPerFrame;
        private IEnumerator _iter;
        private Stopwatch _watch;

        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningAction"/> class.
        /// </summary>
        /// <param name="action">The action to execute. This should yield at intervals to allow distributed execution.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame.</param>
        public LongRunningAction(Func<IEnumerator> action, int maxMillisecondUsedPerFrame)
            : this(action, maxMillisecondUsedPerFrame, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningAction"/> class.
        /// </summary>
        /// <param name="action">The action to execute. This should yield at intervals to allow distributed execution.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame.</param>
        /// <param name="callback">A callback which will be invoked once the action is complete.</param>
        public LongRunningAction(Func<IEnumerator> action, int maxMillisecondUsedPerFrame, Action callback)
        {
            Ensure.ArgumentNotNull(action, "action");

            _action = action;
            _maxMillisecondUsedPerFrame = maxMillisecondUsedPerFrame;
            _callback = callback;
            _watch = new Stopwatch();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningAction"/> class.
        /// </summary>
        /// <param name="action">The action to execute represented by an enumerator, e.g. step-wise execution.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame.</param>
        public LongRunningAction(IEnumerator action, int maxMillisecondUsedPerFrame)
            : this(action, maxMillisecondUsedPerFrame, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningAction"/> class.
        /// </summary>
        /// <param name="action">The action to execute represented by an enumerator, e.g. step-wise execution.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame.</param>
        /// <param name="callback">A callback which will be invoked once the action is complete.</param>
        public LongRunningAction(IEnumerator action, int maxMillisecondUsedPerFrame, Action callback)
        {
            Ensure.ArgumentNotNull(action, "action");

            _iter = action;
            _maxMillisecondUsedPerFrame = maxMillisecondUsedPerFrame;
            _callback = callback;
            _watch = new Stopwatch();
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
            if (_iter == null)
            {
                _iter = _action();
            }

            bool moreWork = true;
            _watch.Reset();
            _watch.Start();
            while (moreWork && _watch.ElapsedMilliseconds < _maxMillisecondUsedPerFrame)
            {
                moreWork = _iter.MoveNext();
            }

            this.repeat = moreWork;
            if (!moreWork)
            {
                _iter = null;

                if (_callback != null)
                {
                    _callback();
                }
            }

            return 0f;
        }
    }
}
