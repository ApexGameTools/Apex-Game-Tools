/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.LoadBalancing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Apex.DataStructures;
    using UnityEngine;

    /// <summary>
    /// A special queue which updates a certain (max) number of items each time its <see cref="Update"/> method is called.
    /// </summary>
    public sealed class LoadBalancedQueue : ILoadBalancer
    {
        private BinaryHeap<LoadBalancerItem> _queue;
        private Stopwatch _watch;

        private Func<float> _time;
        private Func<float> _deltaTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedQueue"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public LoadBalancedQueue(int capacity, bool unscaledTime = false)
            : this(capacity, 0.1f, 20, 4, false, unscaledTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedQueue"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="defaultUpdateInterval">The default update interval to use for items where a specific interval is not specified.</param>
        /// <param name="autoAdjust">Controls whether to automatically adjust <see cref="maxUpdatesPerInterval"/> and <see cref="maxUpdateTimeInMillisecondsPerUpdate"/>,
        /// such that all queued items will be evenly spread across the <see cref="defaultUpdateInterval"/>.</param>
        public LoadBalancedQueue(int capacity, float defaultUpdateInterval, bool autoAdjust, bool unscaledTime = false)
            : this(capacity, defaultUpdateInterval, 20, 4, autoAdjust, unscaledTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedQueue"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="defaultUpdateInterval">The default update interval to use for items where a specific interval is not specified.</param>
        /// <param name="maxUpdatesPerInterval">The maximum number of items to update on each call to <see cref="Update"/>.</param>
        /// <param name="maxUpdateTimeInMillisecondsPerUpdate">The maximum update time in milliseconds that each call to <see cref="Update"/> is allowed to take.</param>
        public LoadBalancedQueue(int capacity, float defaultUpdateInterval, int maxUpdatesPerInterval, int maxUpdateTimeInMillisecondsPerUpdate, bool unscaledTime = false)
            : this(capacity, defaultUpdateInterval, maxUpdatesPerInterval, maxUpdateTimeInMillisecondsPerUpdate, false, unscaledTime)
        {
        }

        private LoadBalancedQueue(int capacity, float defaultUpdateInterval, int maxUpdatesPerInterval, int maxUpdateTimeInMillisecondsPerUpdate, bool autoAdjust, bool unscaledTime)
        {
            this.defaultUpdateInterval = defaultUpdateInterval;
            this.maxUpdatesPerInterval = maxUpdatesPerInterval;
            this.maxUpdateTimeInMillisecondsPerUpdate = maxUpdateTimeInMillisecondsPerUpdate;
            this.autoAdjust = autoAdjust;

            _queue = new BinaryHeap<LoadBalancerItem>(capacity, LoadBalanceItemComparer.instance);
            _watch = new Stopwatch();

            if (unscaledTime)
            {
                _time = () => Time.unscaledTime;
                _deltaTime = () => Time.unscaledDeltaTime;
            }
            else
            {
                _time = () => Time.time;
                _deltaTime = () => Time.deltaTime;
            }
        }

        /// <summary>
        /// Gets or sets the default update interval to use for items where a specific interval is not specified.
        /// </summary>
        /// <value>
        /// The default update interval.
        /// </value>
        public float defaultUpdateInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of items to update on each call to <see cref="Update"/>.
        /// </summary>
        /// <value>
        /// The maximum updates per interval.
        /// </value>
        public int maxUpdatesPerInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum update time in milliseconds that each call to <see cref="Update"/> is allowed to take.
        /// </summary>
        /// <value>
        /// The maximum update time in milliseconds per update.
        /// </value>
        public int maxUpdateTimeInMillisecondsPerUpdate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically adjust <see cref="maxUpdatesPerInterval"/> and <see cref="maxUpdateTimeInMillisecondsPerUpdate"/>,
        /// such that all queued items will be evenly spread across the <see cref="defaultUpdateInterval"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to automatically adjust; otherwise, <c>false</c>.
        /// </value>
        public bool autoAdjust
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the accumulated number of seconds the updates were overdue this frame, i.e. sum of all updates.
        /// </summary>
        public float updatesOverdueByTotal
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time used on the last update.
        /// </summary>
        /// <value>
        /// The update milliseconds used.
        /// </value>
        public long updateMillisecondsUsed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the updated items count, i.e. how many items were updated last frame
        /// </summary>
        /// <value>
        /// The updated items count.
        /// </value>
        public int updatedItemsCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the item count, i.e. the number of items currently in the load balancer.
        /// </summary>
        /// <value>
        /// The item count.
        /// </value>
        public int itemCount
        {
            get { return _queue.count; }
        }

        /// <summary>
        /// Preallocates the load balancer to the specified preallocation.
        /// </summary>
        /// <param name="preallocation">The preallocation.</param>
        public void Preallocate(int preallocation)
        {
            _queue.Resize(preallocation);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public ILoadBalancedHandle Add(ILoadBalanced item)
        {
            return Add(item, this.defaultUpdateInterval, 0.0f);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="delayFirstUpdate">if set to <c>true</c> the item will be scheduled for its first update <see cref="defaultUpdateInterval"/> into the future, otherwise its first update will be as soon as possible.</param>
        public ILoadBalancedHandle Add(ILoadBalanced item, bool delayFirstUpdate)
        {
            return Add(item, this.defaultUpdateInterval, delayFirstUpdate);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="interval">The interval between updates. This overrides the <see cref="defaultUpdateInterval"/>.</param>
        public ILoadBalancedHandle Add(ILoadBalanced item, float interval)
        {
            return Add(item, interval, 0.0f);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="interval">The interval between updates. This overrides the <see cref="defaultUpdateInterval"/>.</param>
        /// <param name="delayFirstUpdate">if set to <c>true</c> the item will be scheduled for its first update <paramref name="interval"/> into the future, otherwise its first update will be as soon as possible.</param>
        public ILoadBalancedHandle Add(ILoadBalanced item, float interval, bool delayFirstUpdate)
        {
            var delay = delayFirstUpdate ? interval : 0.0f;
            return Add(item, interval, delay);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="interval">The interval between updates. This overrides the <see cref="defaultUpdateInterval"/>.</param>
        /// <param name="delayFirstUpdateBy">The delay by which the first update of the item will be scheduled.</param>
        public ILoadBalancedHandle Add(ILoadBalanced item, float interval, float delayFirstUpdateBy)
        {
            var now = _time();
            var queueItem = new LoadBalancerItem
            {
                parent = this,
                lastUpdate = now,
                nextUpdate = now + delayFirstUpdateBy,
                interval = interval,
                item = item
            };

            _queue.Add(queueItem);
            return queueItem;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Remove(ILoadBalanced item)
        {
            var lbi = _queue.Remove(o => o.item == item);
            if (lbi != null)
            {
                lbi.Dispose();
            }
        }

        /// <summary>
        /// Clears the load balancer of all scheduled tasks.
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
        }

        /// <summary>
        /// Updates as many items as possible within the constraints of <see cref="maxUpdatesPerInterval"/> and <see cref="maxUpdateTimeInMillisecondsPerUpdate"/>.
        /// Items are updated only when their time is up, that is when they have not been updated for the interval with which they where added.
        /// </summary>
        public void Update()
        {
            if (!_queue.hasNext)
            {
                return;
            }

            var now = _time();
            _watch.Reset();
            _watch.Start();

            var maxUpdates = this.maxUpdatesPerInterval;
            int updateCount = 0;
            float overDue = 0.0f;

            if (autoAdjust)
            {
                var framesPerInterval = this.defaultUpdateInterval / _deltaTime();
                maxUpdates = Mathf.CeilToInt(_queue.count / framesPerInterval);
            }

            var item = _queue.Peek();
            while ((updateCount++ < maxUpdates) && (item.nextUpdate <= now) && (this.autoAdjust || (_watch.ElapsedMilliseconds < this.maxUpdateTimeInMillisecondsPerUpdate)))
            {
                var deltaTime = now - item.lastUpdate;
                overDue += (deltaTime - item.interval);

                var nextInterval = item.item.ExecuteUpdate(deltaTime, item.interval).GetValueOrDefault(item.interval);

                if (item.item.repeat)
                {
                    //Next interval is the suggested interval or the default. It cannot be 0 since that would lead to continuous updates in this loop.
                    nextInterval = Mathf.Max(nextInterval, 0.01f);

                    item.lastUpdate = now;
                    item.nextUpdate = now + nextInterval;
                    _queue.ReheapifyDownFrom(0);
                }
                else
                {
                    var lbi = _queue.Remove();
                    lbi.Dispose();
                }

                if (!_queue.hasNext)
                {
                    break;
                }

                item = _queue.Peek();
            }

            this.updatedItemsCount = updateCount - 1;
            this.updatesOverdueByTotal = overDue;
            this.updateMillisecondsUsed = _watch.ElapsedMilliseconds;
        }

        private void Remove(LoadBalancerItem item)
        {
            _queue.Remove(item);
        }

        private class LoadBalancerItem : ILoadBalancedHandle
        {
            private bool _isDisposed;

            internal LoadBalancedQueue parent;

            internal float nextUpdate;

            internal float lastUpdate;

            internal float interval;

            public ILoadBalanced item { get; set; }

            public bool isDisposed
            {
                get { return _isDisposed; }
            }

            public bool isPaused
            {
                get { return nextUpdate == float.MaxValue; }
            }

            public void Stop()
            {
                if (!_isDisposed)
                {
                    parent.Remove(this);
                    Dispose();
                }
            }

            public void Pause()
            {
                if (_isDisposed)
                {
                    return;
                }

                var now = parent._time();
                var remaining = (nextUpdate - now);

                //We just use the lastUpdate var to store remaining time, since it will be reestablished to its proper value on resume.
                lastUpdate = remaining;
                nextUpdate = float.MaxValue;
                parent._queue.ReheapifyDownFrom(this);
            }

            public void Resume()
            {
                if (_isDisposed || !isPaused)
                {
                    return;
                }

                var now = parent._time();

                //lastUpdate stores the remaining time at the time of pause
                nextUpdate = now + lastUpdate;
                lastUpdate = nextUpdate - interval;
                parent._queue.ReheapifyUpFrom(this);
            }

            internal void Dispose()
            {
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    parent = null;
                }
            }
        }

        private sealed class LoadBalanceItemComparer : IComparer<LoadBalancerItem>
        {
            public static readonly IComparer<LoadBalancerItem> instance = new LoadBalanceItemComparer();

            public int Compare(LoadBalancerItem x, LoadBalancerItem y)
            {
                return y.nextUpdate.CompareTo(x.nextUpdate);
            }
        }
    }
}