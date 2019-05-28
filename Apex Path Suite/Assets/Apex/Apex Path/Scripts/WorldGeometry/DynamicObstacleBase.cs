/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Steering;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Represents an obstacle with a dynamic nature, meaning it can be an obstacle to only some, only at certain times, etc.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract partial class DynamicObstacleBase : ExtendedMonoBehaviour, IDynamicObstacle, ILoadBalanced, IHandleMessage<GridStatusMessage>
    {
        /// <summary>
        /// Controls when the obstacle updates its state, and thereby its associated grid.
        /// </summary>
        [Tooltip("Controls when the obstacle updates its state, and thereby its associated grid.")]
        public UpdateMode updateMode;

        /// <summary>
        /// Determines how far in the obstacles direction of movement, that cells will be considered blocked.
        /// </summary>
        [MinCheck(0f, tooltip = "Determines how far in the obstacles direction of movement, that cells will be considered blocked.")]
        public float velocityPredictionFactor = 1.5f;

        /// <summary>
        /// Controls whether the obstacle will look for a velocity source on the parent game object if one is not found on its own.
        /// </summary>
        [Tooltip("Controls whether the obstacle will look for a velocity source on the parent game object if one is not found on its own.")]
        public bool resolveVelocityFromParent = false;

        /// <summary>
        /// Setting this to true, will stop the dynamic updates if the obstacle remains stationary for <see cref="stationaryThresholdSeconds"/> seconds.
        /// </summary>
        [Tooltip("Setting this to true, will stop the dynamic updates if the obstacle remains stationary for 'Stationary Threshold Seconds' seconds.")]
        public bool stopUpdatingIfStationary = false;

        /// <summary>
        /// The amount of seconds after which dynamic updates on the obstacle will stop if <see cref="stopUpdatingIfStationary"/> is set to <c>true</c>.
        /// </summary>
        [MinCheck(0f, tooltip = "The amount of seconds after which dynamic updates on the obstacle will stop.")]
        public float stationaryThresholdSeconds = 5.0f;

        /// <summary>
        /// Controls whether the obstacle sensitivity range of the grid will be used by this obstacle.
        /// </summary>
        [Tooltip("Controls whether the obstacle sensitivity range of the grid will be used by this obstacle.")]
        public bool useGridObstacleSensitivity = true;

        /// <summary>
        /// When <see cref="useGridObstacleSensitivity"/> is false, this sensitivity is used instead.
        /// </summary>
        [MinCheck(0f, tooltip = "The custom obstacle sensitivity range to use.")]
        public float customSensitivity = 0f;

        /// <summary>
        /// Setting this to a value other than 0, will override the default update interval of the load balancer.
        /// </summary>
        [MinCheck(0f, tooltip = "Setting this to a value other than 0, will override the default update interval of the load balancer.")]
        public float customUpdateInterval = 0.0f;

        /// <summary>
        /// Controls whether the dynamic obstacle will react to grids being dynamically initialized and/or disabled.
        /// If you use dynamic grid initialization this should be set to true.
        /// </summary>
        [Tooltip("If checked, the Dynamic Obstacle will react to grids being disabled / initialized at runtime. If you use dynamic grid initialization this should be checked.")]
        public bool supportDynamicGrids = false;

        /// <summary>
        /// Controls whether the dynamic obstacle will trigger path replanning for unit's in the grid sections it influences.
        /// </summary>
        [Tooltip("Controls whether the dynamic obstacle will trigger path replanning for unit's in the grid sections it influences.")]
        public bool causesReplanning = true;

        /// <summary>
        /// The cached transform
        /// </summary>
        protected Transform _transform;
        private IMovingObject _mover;
        private Rigidbody _rigidBody;
        private float _hasBeenStationaryForSeconds;
        private bool _isScheduledForUpdates;

        [SerializeField, AttributeProperty("Exceptions", "Units with one or more matching attributes will not consider this obstacle when path finding, e.g. useful for doors etc..")]
        private int _exceptionsMask;

        /// <summary>
        /// How the obstacle updates its state, and thereby its associated grid.
        /// </summary>
        public enum UpdateMode
        {
            /// <summary>
            /// The obstacle is repeatedly updated at the default or <see cref="customUpdateInterval"/>
            /// </summary>
            OnInterval,

            /// <summary>
            /// The obstacle is updated once on start, and then only on request by calling <see cref="ActivateUpdates"/>
            /// </summary>
            OnRequest
        }

        /// <summary>
        /// Gets the position of the component.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 position
        {
            get { return _transform.position; }
        }

        /// <summary>
        /// Gets the attribute mask that defines the attributes for which this obstacle will not be considered an obstacle.
        /// </summary>
        /// <value>
        /// The exceptions mask.
        /// </value>
        public AttributeMask exceptionsMask
        {
            get
            {
                return _exceptionsMask;
            }

            set
            {
                if (_exceptionsMask != value)
                {
                    //If changed at runtime immediately do an update
                    if (_transform != null)
                    {
                        UpdateCells(false);
                        _exceptionsMask = value;
                        UpdateCells(true);
                    }
                    else
                    {
                        _exceptionsMask = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this entity is repeatedly updated each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity will be updated each interval; <c>false</c> if it will only be updated once and then removed from the load balancer.
        /// </value>
        public bool repeat
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is velocity enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is velocity enabled; otherwise, <c>false</c>.
        /// </value>
        protected bool isVelocityEnabled
        {
            get;
            private set;
        }

        private void Awake()
        {
            _transform = this.transform;

            if (this.resolveVelocityFromParent && _transform.parent != null)
            {
                _mover = _transform.parent.As<IMovingObject>();
                _rigidBody = _transform.parent.GetComponent<Rigidbody>();
            }

            if (_mover == null && _rigidBody == null)
            {
                _mover = this.As<IMovingObject>();
                _rigidBody = this.GetComponent<Rigidbody>();
            }

            this.isVelocityEnabled = (_mover != null || (_rigidBody != null && !_rigidBody.isKinematic));

            OnAwake();

            if ((this.updateMode == UpdateMode.OnRequest) && this.isVelocityEnabled)
            {
                Debug.LogWarning("Please note that this obstacle is marked to update on request, and will not be automatically updating after moving.");
            }
        }

        /// <summary>
        /// Override to render the visualization of the dynamic obstacle's coverage.
        /// </summary>
        public virtual void RenderVisualization()
        {
            /* NOOP */
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        protected virtual void OnAwake()
        {
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            if (this.supportDynamicGrids)
            {
                GameServices.messageBus.Subscribe(this);

                var grid = GridManager.instance.GetGrid(_transform.position);
                if (grid == null)
                {
                    return;
                }
            }

            EnableInternal();
        }

        /// <summary>
        /// Updates the cells.
        /// </summary>
        /// <param name="block">if set to <c>true</c> blocked cells will be calculated; otherwise only unblocking will occur.</param>
        protected abstract void UpdateCells(bool block);

        private void OnDisable()
        {
            if (this.supportDynamicGrids)
            {
                GameServices.messageBus.Unsubscribe(this);
            }

            DisableInternal();
        }

        private void EnableInternal()
        {
            var repeat = (this.updateMode == UpdateMode.OnInterval);
            InitializeState(this.customUpdateInterval, repeat);
        }

        private void DisableInternal()
        {
            if (_isScheduledForUpdates)
            {
                NavLoadBalancer.dynamicObstacles.Remove(this);
            }

            _isScheduledForUpdates = false;

            UpdateCells(false);
        }

        /// <summary>
        /// Explicitly starts updating the dynamic obstacle, making it reevaluate its state.
        /// </summary>
        /// <param name="interval">The interval by which to update. Pass null to use the default <see cref="customUpdateInterval"/>.</param>
        /// <param name="repeat">if set to <c>true</c> it will repeatedly update every <paramref name="interval" /> otherwise it will update only once.</param>
        public void ActivateUpdates(float? interval, bool repeat)
        {
            if (_isScheduledForUpdates)
            {
                NavLoadBalancer.dynamicObstacles.Remove(this);
                _isScheduledForUpdates = false;
            }

            InitializeState(interval.GetValueOrDefault(this.customUpdateInterval), repeat);
        }

        /// <summary>
        /// Toggles the obstacle on or off. This is preferred to enabling/disabling the component if it is a regularly recurring action.
        /// </summary>
        /// <param name="active">if set to <c>true</c> the obstacle is toggle on, otherwise off.</param>
        public void Toggle(bool active)
        {
            NavLoadBalancer.dynamicObstacles.Add(new ToggleAction(this, active));
        }

        float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
        {
            if (this.repeat && !_transform.hasChanged)
            {
                if (this.stopUpdatingIfStationary)
                {
                    _hasBeenStationaryForSeconds += deltaTime;
                    if (_hasBeenStationaryForSeconds > this.stationaryThresholdSeconds)
                    {
                        _isScheduledForUpdates = false;
                        this.repeat = false;
                    }
                }

                return null;
            }

            _transform.hasChanged = false;
            _hasBeenStationaryForSeconds = 0.0f;
            _isScheduledForUpdates = this.repeat;

            UpdateCells(true);

            return null;
        }

        void IHandleMessage<GridStatusMessage>.Handle(GridStatusMessage message)
        {
            if (!message.gridBounds.Contains(_transform.position))
            {
                return;
            }

            switch (message.status)
            {
                case GridStatusMessage.StatusCode.DisableComplete:
                {
                    DisableInternal();
                    break;
                }

                case GridStatusMessage.StatusCode.InitializationComplete:
                {
                    if (this.enabled)
                    {
                        EnableInternal();
                    }

                    break;
                }
            }
        }

        private void InitializeState(float updateInterval, bool repeat)
        {
            if (_isScheduledForUpdates)
            {
                return;
            }

            //Ensure that the obstacle updates the grid at least once even when stationary (mainly in case of runtime disable/enable of grid)
            _transform.hasChanged = true;
            _isScheduledForUpdates = true;
            this.repeat = repeat;

            if (updateInterval > 0.0f)
            {
                NavLoadBalancer.dynamicObstacles.Add(this, updateInterval);
            }
            else
            {
                NavLoadBalancer.dynamicObstacles.Add(this);
            }
        }

        /// <summary>
        /// Gets the velocity of the obstacle.
        /// </summary>
        /// <returns></returns>
        protected Vector3 GetVelocity()
        {
            Vector3 velocity = Vector3.zero;

            if (_mover != null)
            {
                velocity = _mover.velocity;
            }

            if (_rigidBody != null)
            {
                velocity = velocity + _rigidBody.velocity;
            }

            return velocity * this.velocityPredictionFactor;
        }

        private class ToggleAction : ILoadBalanced
        {
            private DynamicObstacleBase _target;
            private bool _block;

            public ToggleAction(DynamicObstacleBase target, bool block)
            {
                _target = target;
                _block = block;
            }

            public bool repeat
            {
                get { return false; }
            }

            public float? ExecuteUpdate(float deltaTime, float nextInterval)
            {
                _target.UpdateCells(_block);

                return null;
            }
        }
    }
}
