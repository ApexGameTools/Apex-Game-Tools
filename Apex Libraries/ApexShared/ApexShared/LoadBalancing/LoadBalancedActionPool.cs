/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.LoadBalancing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Apex.Utilities;

    /// <summary>
    /// Extension for executing tasks in the load balancer.
    /// </summary>
    public static class LoadBalancedActionPool
    {
        private static Queue<RecycledOneTimeAction> _oneTimeActions;
        private static Queue<RecycledLongRunningAction> _longActions;
        private static Queue<RecycledAction> _actions;

        /// <summary>
        /// Preallocates the recycled one time action queue to the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public static void PreallocateOneTimeActions(int capacity)
        {
            _oneTimeActions = new Queue<RecycledOneTimeAction>(capacity);
        }

        /// <summary>
        /// Preallocates the long running actions queue to the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public static void PreallocateLongRunningActions(int capacity)
        {
            _longActions = new Queue<RecycledLongRunningAction>(capacity);
        }

        /// <summary>
        /// Preallocates the actions queue to the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public static void PreallocateActions(int capacity)
        {
            _actions = new Queue<RecycledAction>(capacity);
        }

        /// <summary>
        /// Executes the specified action once.
        /// </summary>
        /// <param name="lb">The load balancer.</param>
        /// <param name="action">The action.</param>
        /// <param name="delay">The delay until the action is executed.</param>
        public static void ExecuteOnce(this ILoadBalancer lb, Action action, float delay = 0f)
        {
            Ensure.ArgumentNotNull(action, "action");

            if (_oneTimeActions == null)
            {
                _oneTimeActions = new Queue<RecycledOneTimeAction>(1);
            }

            RecycledOneTimeAction ota;
            if (_oneTimeActions.Count > 0)
            {
                ota = _oneTimeActions.Dequeue();
                ota.action = action;
            }
            else
            {
                ota = new RecycledOneTimeAction
                {
                    action = action
                };
            }

            if (delay > 0f)
            {
                lb.Add(ota, delay, true);
            }
            else
            {
                lb.Add(ota);
            }
        }

        /// <summary>
        /// Executes the specified action as long as it returns <c>true</c>.
        /// </summary>
        /// <param name="lb">The load balancer.</param>
        /// <param name="action">The action.</param>
        /// <param name="delayFirstUpdate">if set to <c>true</c> the first execution of the action will be delayed by <paramref name="interval"/>, otherwise it will run on the next frame.</param>
        /// <returns>A handle that can be used to Stop, pause and resume the action.</returns>
        public static ILoadBalancedHandle Execute(this ILoadBalancer lb, Func<float, bool> action, bool delayFirstUpdate = false)
        {
            var ota = GetAction(action);

            return lb.Add(ota, delayFirstUpdate);
        }

        /// <summary>
        /// Executes the specified action as long as it returns <c>true</c>.
        /// </summary>
        /// <param name="lb">The load balancer.</param>
        /// <param name="action">The action.</param>
        /// <param name="interval">The interval between the action being executed.</param>
        /// <param name="delayFirstUpdate">if set to <c>true</c> the first execution of the action will be delayed by <paramref name="interval"/>, otherwise it will run on the next frame.</param>
        /// <returns>A handle that can be used to Stop, pause and resume the action.</returns>
        public static ILoadBalancedHandle Execute(this ILoadBalancer lb, Func<float, bool> action, float interval, bool delayFirstUpdate = false)
        {
            var ota = GetAction(action);

            return lb.Add(ota, interval, delayFirstUpdate);
        }

        /// <summary>
        /// Executes the specified action as long as it returns <c>true</c>.
        /// </summary>
        /// <param name="lb">The load balancer.</param>
        /// <param name="action">The action.</param>
        /// <param name="interval">The interval between the action being executed.</param>
        /// <param name="delayFirstUpdateBy">The delay by which the first execution of the action will be scheduled.</param>
        /// <returns>A handle that can be used to Stop, pause and resume the action.</returns>
        public static ILoadBalancedHandle Execute(this ILoadBalancer lb, Func<float, bool> action, float interval, float delayFirstUpdateBy)
        {
            var ota = GetAction(action);

            return lb.Add(ota, interval, delayFirstUpdateBy);
        }

        /// <summary>
        /// Executes the specified long running action.
        /// </summary>
        /// <param name="lb">The load balancer.</param>
        /// <param name="longRunningAction">The long running action, i.e. an action that will execute in steps by means of an enumerator.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum milliseconds to use per frame.</param>
        /// <returns>>A handle that can be used to Stop, pause and resume the action.</returns>
        public static ILoadBalancedHandle Execute(this ILoadBalancer lb, IEnumerator longRunningAction, int maxMillisecondsUsedPerFrame)
        {
            Ensure.ArgumentNotNull(longRunningAction, "longRunningAction");

            if (_longActions == null)
            {
                _longActions = new Queue<RecycledLongRunningAction>(1);
            }

            RecycledLongRunningAction lra;
            if (_longActions.Count > 0)
            {
                lra = _longActions.Dequeue();
                lra.iter = longRunningAction;
                lra.maxMillisecondsUsedPerFrame = maxMillisecondsUsedPerFrame;
            }
            else
            {
                lra = new RecycledLongRunningAction
                {
                    iter = longRunningAction,
                    maxMillisecondsUsedPerFrame = maxMillisecondsUsedPerFrame
                };
            }

            return lb.Add(lra);
        }

        private static RecycledAction GetAction(Func<float, bool> action)
        {
            Ensure.ArgumentNotNull(action, "action");

            if (_actions == null)
            {
                _actions = new Queue<RecycledAction>(1);
            }

            RecycledAction ota;
            if (_actions.Count > 0)
            {
                ota = _actions.Dequeue();
                ota.action = action;
            }
            else
            {
                ota = new RecycledAction
                {
                    action = action
                };
            }

            return ota;
        }

        private static void Return(RecycledOneTimeAction action)
        {
            action.action = null;
            _oneTimeActions.Enqueue(action);
        }

        private static void Return(RecycledAction action)
        {
            action.action = null;
            _actions.Enqueue(action);
        }

        private static void Return(RecycledLongRunningAction action)
        {
            action.iter = null;
            _longActions.Enqueue(action);
        }

        private class RecycledOneTimeAction : ILoadBalanced
        {
            private Action _action;

            internal Action action
            {
                get { return _action; }
                set { _action = value; }
            }

            bool ILoadBalanced.repeat
            {
                get { return false; }
            }

            float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
            {
                _action();
                LoadBalancedActionPool.Return(this);
                return null;
            }
        }

        private class RecycledLongRunningAction : ILoadBalanced
        {
            private readonly Stopwatch _watch = new Stopwatch();
            private IEnumerator _iter;
            private int _maxMillisecondsUsedPerFrame;

            internal RecycledLongRunningAction()
            {
                this.repeat = true;
            }

            internal IEnumerator iter
            {
                get { return _iter; }
                set { _iter = value; }
            }

            internal int maxMillisecondsUsedPerFrame
            {
                get { return _maxMillisecondsUsedPerFrame; }
                set { _maxMillisecondsUsedPerFrame = value; }
            }

            public bool repeat
            {
                get;
                private set;
            }

            float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
            {
                bool moreWork = true;
                _watch.Reset();
                _watch.Start();
                while (moreWork && _watch.ElapsedMilliseconds < _maxMillisecondsUsedPerFrame)
                {
                    moreWork = _iter.MoveNext();
                }

                this.repeat = moreWork;
                if (!moreWork)
                {
                    LoadBalancedActionPool.Return(this);
                }

                return 0f;
            }
        }

        private class RecycledAction : ILoadBalanced
        {
            private Func<float, bool> _action;

            internal RecycledAction()
            {
                this.repeat = true;
            }

            internal Func<float, bool> action
            {
                get { return _action; }
                set { _action = value; }
            }

            public bool repeat
            {
                get;
                private set;
            }

            float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
            {
                this.repeat = _action(deltaTime);
                if (!this.repeat)
                {
                    LoadBalancedActionPool.Return(this);
                }

                return null;
            }
        }
    }
}