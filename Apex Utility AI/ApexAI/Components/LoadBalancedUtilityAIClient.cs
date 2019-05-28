/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Components
{
    using System;
    using Apex.LoadBalancing;

    /// <summary>
    /// An AI client implementation that schedules the AI to run on interval in the <see cref="LoadBalancer"/>.
    /// </summary>
    public sealed class LoadBalancedUtilityAIClient : UtilityAIClient, ILoadBalanced
    {
        private ILoadBalancedHandle _lbHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedUtilityAIClient"/> class, with an execution interval of 1 second and no start delay.
        /// </summary>
        /// <param name="aiId">The AI id.</param>
        /// <param name="contextProvider">The context provider.</param>
        public LoadBalancedUtilityAIClient(Guid aiId, IContextProvider contextProvider)
            : this(aiId, contextProvider, 1f, 1f, 0f, 0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedUtilityAIClient"/> class, with an execution interval of 1 second and no start delay.
        /// </summary>
        /// <param name="ai">The AI.</param>
        /// <param name="contextProvider">The context provider.</param>
        public LoadBalancedUtilityAIClient(IUtilityAI ai, IContextProvider contextProvider)
            : this(ai, contextProvider, 1f, 1f, 0f, 0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedUtilityAIClient"/> class.
        /// </summary>
        /// <param name="aiId">The AI identifier.</param>
        /// <param name="contextProvider">The context provider.</param>
        /// <param name="executionIntervalMin">The minimum execution interval in seconds.</param>
        /// <param name="executionIntervalMax">The maximum execution interval in seconds.</param>
        /// <param name="startDelayMin">The minimum number of seconds to delay the initial execution of the AI.</param>
        /// <param name="startDelayMax">The maximum number of seconds to delay the initial execution of the AI.</param>
        public LoadBalancedUtilityAIClient(Guid aiId, IContextProvider contextProvider, float executionIntervalMin, float executionIntervalMax, float startDelayMin, float startDelayMax)
            : base(aiId, contextProvider)
        {
            this.executionIntervalMin = executionIntervalMin;
            this.executionIntervalMax = executionIntervalMax;
            this.startDelayMin = startDelayMin;
            this.startDelayMax = startDelayMax;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedUtilityAIClient"/> class.
        /// </summary>
        /// <param name="ai">The AI.</param>
        /// <param name="contextProvider">The context provider.</param>
        /// <param name="executionIntervalMin">The minimum execution interval in seconds.</param>
        /// <param name="executionIntervalMax">The maximum execution interval in seconds.</param>
        /// <param name="startDelayMin">The minimum number of seconds to delay the initial execution of the AI.</param>
        /// <param name="startDelayMax">The maximum number of seconds to delay the initial execution of the AI.</param>
        public LoadBalancedUtilityAIClient(IUtilityAI ai, IContextProvider contextProvider, float executionIntervalMin, float executionIntervalMax, float startDelayMin, float startDelayMax)
            : base(ai, contextProvider)
        {
            this.executionIntervalMin = executionIntervalMin;
            this.executionIntervalMax = executionIntervalMax;
            this.startDelayMin = startDelayMin;
            this.startDelayMax = startDelayMax;
        }

        /// <summary>
        /// Gets or sets the minimum execution interval in seconds.
        /// If <see cref="executionIntervalMin"/> and <see cref="executionIntervalMax"/> differ, the actual interval will be a random value in between the two (inclusive).
        /// </summary>
        public float executionIntervalMin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum execution interval in seconds.
        /// If <see cref="executionIntervalMin"/> and <see cref="executionIntervalMax"/> differ, the actual interval will be a random value in between the two (inclusive).
        /// </summary>
        public float executionIntervalMax
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum number of seconds to delay the initial execution of the AI.
        /// If <see cref="startDelayMin"/> and <see cref="startDelayMax"/> differ, the actual delay will be a random value in between the two (inclusive).
        /// </summary>
        public float startDelayMin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of seconds to delay the initial execution of the AI.
        /// If <see cref="startDelayMin"/> and <see cref="startDelayMax"/> differ, the actual delay will be a random value in between the two (inclusive).
        /// </summary>
        public float startDelayMax
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether to repeatedly update this entity each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        bool ILoadBalanced.repeat
        {
            get { return true; }
        }

        /// <summary>
        /// Starts the AI.
        /// </summary>
        protected override void OnStart()
        {
            var delay = this.startDelayMin;
            if (delay != this.startDelayMax)
            {
                delay = UnityEngine.Random.Range(delay, this.startDelayMax);
            }

            var interval = this.executionIntervalMin;
            if (interval != this.executionIntervalMax)
            {
                interval = UnityEngine.Random.Range(interval, this.executionIntervalMax);
            }

            _lbHandle = AILoadBalancer.aiLoadBalancer.Add(this, interval, delay);
        }

        /// <summary>
        /// Called after <see cref="IUtilityAIClient.Stop"/>.
        /// </summary>
        protected override void OnStop()
        {
            if (_lbHandle != null)
            {
                _lbHandle.Stop();
                _lbHandle = null;
            }
        }

        /// <summary>
        /// Called after <see cref="IUtilityAIClient.Pause"/>.
        /// </summary>
        protected override void OnPause()
        {
            if (_lbHandle != null)
            {
                _lbHandle.Pause();
            }
        }

        /// <summary>
        /// Called after <see cref="IUtilityAIClient.Resume"/>.
        /// </summary>
        protected override void OnResume()
        {
            if (_lbHandle != null)
            {
                _lbHandle.Resume();
            }
        }

        /// <summary>
        /// Executes the update.
        /// </summary>
        /// <param name="deltaTime">The delta time, i.e. the time passed since the last update.</param>
        /// <param name="nextInterval">The time that will pass until the next update.</param>
        /// <returns>
        /// Can return the next interval by which the update should run. To use the default interval return null.
        /// </returns>
        float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
        {
            Execute();

            if (this.executionIntervalMin != this.executionIntervalMax)
            {
                return UnityEngine.Random.Range(this.executionIntervalMin, this.executionIntervalMax);
            }

            return null;
        }
    }
}
