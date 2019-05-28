/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using System;
    using System.Collections.Generic;
    using Apex.LoadBalancing;
    using UnityEngine;

    /// <summary>
    /// A visualizer to show the load balancers in use are performing. Use this to ensure that load balancers have been configured appropriately to handle the number of items they handle.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Debugging/LoadBalancer Performance Visualizer", 1014)]
    [ApexComponent("Debugging")]
    public class LoadBalancerPerformanceVisualizer : MonoBehaviour
    {
        private PerformanceData[] _data;

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public PerformanceData[] data
        {
            get { return _data; }
        }

        private void Awake()
        {
            var dataList = new List<PerformanceData>();

            var source = this.GetComponent<LoadBalancerComponent>();
            if (source == null)
            {
                Debug.LogWarning("LoadBalancer Performance Visualizer must reside on the same GameObject as the Load Balancer.");
                this.enabled = false;
            }
            else
            {
                foreach (var cfg in source.configurations)
                {
                    dataList.Add(new PerformanceData(cfg.associatedLoadBalancer, cfg.targetLoadBalancer));
                }
            }

            _data = dataList.ToArray();
        }

        private void Update()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i].Update();
            }
        }

        /// <summary>
        /// Class encapsulating load balancer performance data
        /// </summary>
        public class PerformanceData
        {
            private long _updateCount;
            private LoadBalancedQueue _source;

            private float _summedUpdatesOverdueAverage;
            private float _summedUpdateMillisecondsUsed;
            private float _summedUpdatedItemsCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="PerformanceData"/> class.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <param name="loadBalancerName">The name of the load balancer</param>
            public PerformanceData(LoadBalancedQueue source, string loadBalancerName)
            {
                _source = source;
                this.loadBalancerName = loadBalancerName;
            }

            /// <summary>
            /// Gets the name of the load balancer.
            /// </summary>
            /// <value>
            /// The name of the load balancer.
            /// </value>
            public string loadBalancerName
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the items count.
            /// </summary>
            /// <value>
            /// The items count.
            /// </value>
            public int itemsCount
            {
                get { return _source.itemCount; }
            }

            /// <summary>
            /// Gets the frame updates overdue average.
            /// </summary>
            /// <value>
            /// The frame updates overdue average.
            /// </value>
            public float frameUpdatesOverdueAverage
            {
                get
                {
                    if (_source.updatedItemsCount == 0)
                    {
                        return 0.0f;
                    }

                    return Mathf.Clamp((_source.updatesOverdueByTotal / _source.updatedItemsCount) - 0.02f, 0.0f, float.MaxValue);
                }
            }

            /// <summary>
            /// Gets the frame update milliseconds used.
            /// </summary>
            /// <value>
            /// The frame update milliseconds used.
            /// </value>
            public long frameUpdateMillisecondsUsed
            {
                get { return _source.updateMillisecondsUsed; }
            }

            /// <summary>
            /// Gets the frame updated items count.
            /// </summary>
            /// <value>
            /// The frame updated items count.
            /// </value>
            public int frameUpdatedItemsCount
            {
                get { return _source.updatedItemsCount; }
            }

            /// <summary>
            /// Gets the average updates overdue average.
            /// </summary>
            /// <value>
            /// The average updates overdue average.
            /// </value>
            public float averageUpdatesOverdueAverage
            {
                get { return _summedUpdatesOverdueAverage / Math.Max(_updateCount, 1); }
            }

            /// <summary>
            /// Gets the average update milliseconds used.
            /// </summary>
            /// <value>
            /// The average update milliseconds used.
            /// </value>
            public float averageUpdateMillisecondsUsed
            {
                get { return _summedUpdateMillisecondsUsed / Math.Max(_updateCount, 1); }
            }

            /// <summary>
            /// Gets the average updated items count.
            /// </summary>
            /// <value>
            /// The average updated items count.
            /// </value>
            public float averageUpdatedItemsCount
            {
                get { return _summedUpdatedItemsCount / Math.Max(_updateCount, 1); }
            }

            internal void Update()
            {
                if (this.frameUpdatedItemsCount > 0)
                {
                    _summedUpdatesOverdueAverage += this.frameUpdatesOverdueAverage;
                    _summedUpdateMillisecondsUsed += this.frameUpdateMillisecondsUsed;
                    _summedUpdatedItemsCount += this.frameUpdatedItemsCount;
                    _updateCount = (_updateCount % long.MaxValue) + 1;
                }
            }
        }
    }
}
