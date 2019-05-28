/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using System;
    using System.Collections.Generic;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Steering.HeightNavigation;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Main steering controller that combines the input from attached <see cref="SteeringComponent" />s to move the unit.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steerable Unit", 1031)]
    [ApexComponent("Steering")]
    public class SteerableUnitComponent : ExtendedMonoBehaviour, ILoadBalanced, IMovingObject
    {
        private const float AngularSpeedLowerLimit = 0.0001f;

        /// <summary>
        /// The time over which to stop when waiting or when no steering components have any output (as permitted by deceleration capabilities).
        /// </summary>
        [Tooltip("The time over which to stop when waiting or when no steering components have any output (as permitted by deceleration capabilities).")]
        public float stopTimeFrame = 0.2f;

        /// <summary>
        /// The amount of seconds after which the unit will stop if it is stuck.
        /// </summary>
        [Tooltip("The amount of seconds after which the unit will stop if it is stuck.")]
        public float stopIfStuckForSeconds = 3.0f;

        /// <summary>
        /// Determines how the actual velocity is calculated. If set to true, the calculation will be carried out in the load balanced update, which will average the actual velocity over several frames.
        /// If set to false, it will calculate it each frame. The main reason to want to set this to true is if you have custom locomotion logic in place that does not necessarily move the unit every frame, e.g. animations.
        /// </summary>
        [Tooltip("Determines how the actual velocity is calculated. If set to true, the calculation will be carried out in the load balanced update, which will average the actual velocity over several frames. If set to false, it will calculate it each frame.")]
        public bool averageActualVelocity = false;

        [Obsolete("This is handled by the height navigator component on the unit.")]
        public float groundStickynessFactor = 1f;

        [Obsolete("This is handled by the height navigator component on the unit.")]
        public float gravity = -9.81f;

        [Obsolete("This is handled by the height navigator component on the unit.")]
        public float terminalVelocity = 50f;

        private Vector3 _currentVelocity;
        private Vector3 _currentPlanarVelocity;
        private Vector3 _currentSpatialVelocity;
        private float _currentAngularSpeed;

        private bool _waiting;
        private bool _stopped;
        private float _stuckCheckLastMove;

        private Vector3 _lastFramePosition;
        private Vector3 _actualVelocity;

        private Transform _transform;
        private IUnitFacade _unit;
        private IMoveUnits _mover;
        private IHeightNavigator _heights;
        private List<ISteeringBehaviour> _steeringComponents;
        private List<IOrientationBehaviour> _orientationComponents;

        private SteeringOutput _steering;
        private OrientationOutput _orientation;
        private SteeringInput _steeringInput;

        /// <summary>
        /// Gets the speed of the unit in m/s.
        /// </summary>
        /// <value>
        /// The speed.
        /// </value>
        public float speed
        {
            get { return _currentVelocity.magnitude; }
        }

        /// <summary>
        /// Gets the velocity of the unit. This represents the movement force applied to the object. Also see <see cref="actualVelocity"/>.
        /// </summary>
        /// <value>
        /// The velocity.
        /// </value>
        public Vector3 velocity
        {
            get { return _currentVelocity; }
        }

        /// <summary>
        /// Gets the actual velocity of the unit. This may differ from <see cref="velocity"/> in certain scenarios, e.g. during collisions, if being moved by other means etc.
        /// </summary>
        /// <value>
        /// The actual velocity.
        /// </value>
        public Vector3 actualVelocity
        {
            get { return _actualVelocity; }
        }

        /// <summary>
        /// Gets a value indicating whether this object is grounded, i.e. not falling or otherwise raised above its natural base position.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grounded; otherwise, <c>false</c>.
        /// </value>
        public bool isGrounded
        {
            get;
            private set;
        }

        bool ILoadBalanced.repeat
        {
            get { return true; }
        }

        private void Awake()
        {
            this.WarnIfMultipleInstances();

            _transform = this.transform;

            //Resolve the mover
            _mover = this.As<IMoveUnits>();
            if (_mover == null)
            {
                var fact = this.As<IMoveUnitsFactory>();
                var charController = this.GetComponent<CharacterController>();
                var rb = this.GetComponent<Rigidbody>();

                if (fact != null)
                {
                    _mover = fact.Create();
                }
                else if (charController != null)
                {
                    _mover = new CharacterControllerMover(charController);
                }
                else if (rb != null)
                {
                    _mover = new RigidBodyMover(rb);
                }
                else
                {
                    _mover = new DefaultMover(_transform);
                }
            }

            //Height resolver
            _heights = this.As<IHeightNavigator>();
            if (_heights == null)
            {
                _heights = NoHeightNavigator.Instance;
            }

            //Assign unit ref, container for components and steering visitors
            _steeringComponents = new List<ISteeringBehaviour>();
            _orientationComponents = new List<IOrientationBehaviour>();
            _steering = new SteeringOutput();
            _orientation = new OrientationOutput();
            _steeringInput = new SteeringInput();
        }

        /// <summary>
        /// Called on Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            //Get unit ref
            _unit = this.GetUnitFacade();
            _steeringInput.unit = _unit;

            //Init other vars
            _lastFramePosition = _unit.position;
            _stopped = true;

            UpdateInput();
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            //Hook up with the load balancer
            NavLoadBalancer.steering.Add(this);
        }

        private void OnDisable()
        {
            Stop();

            NavLoadBalancer.steering.Remove(this);
        }

        private void FixedUpdate()
        {
            Steer(Time.deltaTime);
        }

        /// <summary>
        /// Registers a steering component.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        public void RegisterSteeringBehavior(ISteeringBehaviour behavior)
        {
            int count = _steeringComponents.Count;
            for (int i = 0; i < count; i++)
            {
                var c = _steeringComponents[i];
                if (c.priority == behavior.priority)
                {
                    var grp = c as SteeringGroup;
                    if (grp == null)
                    {
                        grp = new SteeringGroup(c.priority);
                        _steeringComponents[i] = grp;
                        grp.Add(c);
                    }

                    grp.Add(behavior);
                    return;
                }
            }

            _steeringComponents.Add(behavior);
            _steeringComponents.Sort((a, b) => b.priority.CompareTo(a.priority));
        }

        /// <summary>
        /// Unregisters a steering behavior.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        public void UnregisterSteeringBehavior(ISteeringBehaviour behavior)
        {
            int start = _steeringComponents.Count - 1;
            for (int i = start; i >= 0; i--)
            {
                var c = _steeringComponents[i];
                if (c.priority == behavior.priority)
                {
                    var grp = c as SteeringGroup;
                    if (grp != null)
                    {
                        grp.Remove(behavior);
                    }
                    else
                    {
                        _steeringComponents.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Registers a steering component.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        public void RegisterOrientationBehavior(IOrientationBehaviour behavior)
        {
            _orientationComponents.Add(behavior);
            _orientationComponents.Sort((a, b) => b.priority.CompareTo(a.priority));
        }

        /// <summary>
        /// Unregisters a steering behavior.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        public void UnregisterOrientationBehavior(IOrientationBehaviour behavior)
        {
            _orientationComponents.Remove(behavior);
        }

        /// <summary>
        /// Stops the unit from moving.
        /// </summary>
        public void Stop()
        {
            for (int i = 0; i < _steeringComponents.Count; i++)
            {
                _steeringComponents[i].Stop();
            }

            _waiting = false;
        }

        /// <summary>
        /// Waits the specified seconds before continuing the move.
        /// </summary>
        /// <param name="seconds">The seconds to wait or null to wait until explicitly <see cref="Resume" />d.</param>
        public void Wait(float? seconds)
        {
            _waiting = true;

            if (seconds.HasValue)
            {
                NavLoadBalancer.defaultBalancer.Add(new OneTimeAction((ignored) => this.Resume()), seconds.Value, true);
            }
        }

        /// <summary>
        /// Resumes movements after a <see cref="Wait" />.
        /// </summary>
        public void Resume()
        {
            _stuckCheckLastMove = Time.time;
            _waiting = false;
        }

        private void StopMovement()
        {
            if (_stopped)
            {
                return;
            }

            _unit.SignalStop();

            _stopped = true;

            _currentVelocity = _currentPlanarVelocity = _currentSpatialVelocity = Vector3.zero;
            _steeringInput.currentFullVelocity = _steeringInput.currentPlanarVelocity = _steeringInput.currentSpatialVelocity = Vector3.zero;
            _steeringInput.currentAngularSpeed = _currentAngularSpeed = 0f;

            _mover.Stop();
        }

        private bool IsStuck(float deltaTime)
        {
            if (this.stopIfStuckForSeconds <= 0.0f)
            {
                return false;
            }

            var minSpeed = _unit.minimumSpeed;
            if (_actualVelocity.sqrMagnitude > minSpeed * minSpeed)
            {
                _stuckCheckLastMove = Time.time;
                return false;
            }

            return ((Time.time - _stuckCheckLastMove) > this.stopIfStuckForSeconds);
        }

        float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
        {
            if (this.averageActualVelocity)
            {
                _actualVelocity = (_transform.position - _lastFramePosition) / deltaTime;
                _lastFramePosition = _transform.position;
            }

            if (_stopped || _waiting)
            {
                return null;
            }

            if (IsStuck(deltaTime))
            {
                var msg = new UnitNavigationEventMessage(this.gameObject, UnitNavigationEventMessage.Event.Stuck)
                {
                    destination = _unit.finalDestination.GetValueOrDefault(),
                    pendingWaypoints = _unit.currentWaypoints.ToArray()
                };

                Stop();
                StopMovement();
                GameServices.messageBus.Post(msg);
                return null;
            }

            UpdateInput();

            return null;
        }

        private void Steer(float deltaTime)
        {
            if (!this.averageActualVelocity)
            {
                _actualVelocity = (_transform.position - _lastFramePosition) / deltaTime;
                _lastFramePosition = _transform.position;
            }

            //prepare input and reset output
            var grid = GridManager.instance.GetGrid(_transform.position);
            _steeringInput.grid = grid;
            _steeringInput.deltaTime = deltaTime;

            _steering.Clear();

            if (!_waiting)
            {
                //Get the steering output from all registered components
                var count = _steeringComponents.Count;
                for (int i = 0; i < count; i++)
                {
                    _steeringComponents[i].GetSteering(_steeringInput, _steering);
                    if (_steering.hasOutput)
                    {
                        break;
                    }
                }

                if (_steering.pause)
                {
                    return;
                }
            }

            //Store it locally since we do not want to inadvertently modify the output to have an output if it does not
            var desiredAcceleration = _steering.desiredAcceleration;

            //If we are waiting or no steering components had an output, slow down to a stop
            if (!_steering.hasOutput && !_stopped)
            {
                var timeToTarget = Mathf.Max(this.stopTimeFrame, deltaTime);
                desiredAcceleration = Vector3.ClampMagnitude(-_currentSpatialVelocity / timeToTarget, _steeringInput.maxDeceleration);
            }

            //Adjust the current velocity to the output
            _currentSpatialVelocity = _currentSpatialVelocity + (desiredAcceleration * deltaTime);

            //Check if the velocity is below the minimum to move, provided there is no output from components
            var minSpeed = _unit.minimumSpeed;
            if (_currentSpatialVelocity.sqrMagnitude < minSpeed * minSpeed && !_steering.hasOutput)
            {
                StopMovement();
            }
            else if (_stopped)
            {
                _stuckCheckLastMove = Time.time;
                _stopped = false;
            }

            //Set the current velocity
            var maxSpeed = Mathf.Max(_steering.maxAllowedSpeed, _steeringInput.desiredSpeed);
            _currentSpatialVelocity = Vector3.ClampMagnitude(_currentSpatialVelocity, maxSpeed);
            _currentPlanarVelocity = _currentSpatialVelocity.OnlyXZ();

            _steeringInput.currentSpatialVelocity = _currentSpatialVelocity;
            _steeringInput.currentPlanarVelocity = _currentPlanarVelocity;

            //Adjust to elevation and gravity
            var heightOutput = _heights.GetHeightOutput(_steeringInput, maxSpeed);

            this.isGrounded = heightOutput.isGrounded;

            if (_steering.overrideHeightNavigation)
            {
                _currentVelocity = _currentSpatialVelocity;
            }
            else
            {
                _currentVelocity = heightOutput.finalVelocity;
            }

            //Add additional vertical force if requested
            if (_steering.verticalForce != 0f)
            {
                _currentVelocity.y += _steering.verticalForce * deltaTime;
            }

            //Final velocity has been resolved, now update the input. Doing it here rather than on the next frame ensure that orientation responds to the most accurate values
            _steeringInput.currentFullVelocity = _currentVelocity;

            //Get the orientation component
            _orientation.Clear();
            var orientationCount = _orientationComponents.Count;
            for (int i = 0; i < orientationCount; i++)
            {
                _orientationComponents[i].GetOrientation(_steeringInput, _orientation);
                if (_orientation.hasOutput)
                {
                    break;
                }
            }

            if (Mathf.Abs(_orientation.desiredAngularAcceleration) > 0.01f)
            {
                _currentAngularSpeed = _currentAngularSpeed + (_orientation.desiredAngularAcceleration * deltaTime);
                _currentAngularSpeed = Mathf.Clamp(_currentAngularSpeed, 0f, _steeringInput.desiredAngularSpeed);
                _steeringInput.currentAngularSpeed = _currentAngularSpeed;
            }

            //Do the movement and rotation. We do this even if velocity is 0 since some implementations require this.
            _mover.Move(_currentVelocity, deltaTime);

            if (_currentAngularSpeed > AngularSpeedLowerLimit && _orientation.desiredOrientation.sqrMagnitude > 0f)
            {
                _mover.Rotate(_orientation.desiredOrientation, _currentAngularSpeed, deltaTime);
            }
        }

        private void UpdateInput()
        {
            _steeringInput.gravity = _heights.gravity;
            _steeringInput.maxAcceleration = _unit.maxAcceleration;
            _steeringInput.maxDeceleration = _unit.maxDeceleration;
            _steeringInput.maxAngularAcceleration = _unit.maxAngularAcceleration;
            _steeringInput.desiredSpeed = _unit.GetPreferredSpeed(_currentPlanarVelocity.normalized);
            _steeringInput.desiredAngularSpeed = _unit.maximumAngularSpeed;
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="steerableUnit">The component to clone from.</param>
        public void CloneFrom(SteerableUnitComponent steerableUnit)
        {
            this.stopIfStuckForSeconds = steerableUnit.stopIfStuckForSeconds;
            this.stopTimeFrame = steerableUnit.stopTimeFrame;
        }

        private class DefaultMover : IMoveUnits
        {
            private Transform _transform;

            public DefaultMover(Transform transform)
            {
                _transform = transform;
            }

            public void Move(Vector3 velocity, float deltaTime)
            {
                _transform.position = _transform.position + (velocity * deltaTime);
            }

            public void Rotate(Vector3 targetOrientation, float angularSpeed, float deltaTime)
            {
                var targetRotation = Quaternion.LookRotation(targetOrientation);
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation, targetRotation, angularSpeed * Mathf.Rad2Deg * deltaTime);
            }

            public void Stop()
            {
                /* NOOP */
            }
        }

        private class RigidBodyMover : IMoveUnits
        {
            private Rigidbody _rigidBody;

            public RigidBodyMover(Rigidbody rigidBody)
            {
                _rigidBody = rigidBody;
            }

            public void Move(Vector3 velocity, float deltaTime)
            {
                if (!_rigidBody.isKinematic)
                {
                    _rigidBody.velocity = _rigidBody.angularVelocity = Vector3.zero;
                }

                _rigidBody.MovePosition(_rigidBody.position + (velocity * deltaTime));
            }

            public void Rotate(Vector3 targetOrientation, float angularSpeed, float deltaTime)
            {
                if (targetOrientation.sqrMagnitude == 0f)
                {
                    return;
                }

                var targetRotation = Quaternion.LookRotation(targetOrientation);
                var orientation = Quaternion.RotateTowards(_rigidBody.rotation, targetRotation, angularSpeed * Mathf.Rad2Deg * deltaTime);
                _rigidBody.MoveRotation(orientation);
            }

            public void Stop()
            {
                if (!_rigidBody.isKinematic)
                {
                    _rigidBody.velocity = Vector3.zero;
                    _rigidBody.Sleep();
                }
            }
        }

        private class CharacterControllerMover : IMoveUnits
        {
            private Transform _transform;
            private CharacterController _controller;

            public CharacterControllerMover(CharacterController controller)
            {
                _transform = controller.transform;
                _controller = controller;
            }

            public void Move(Vector3 velocity, float deltaTime)
            {
                _controller.Move(velocity * deltaTime);
            }

            public void Rotate(Vector3 targetOrientation, float angularSpeed, float deltaTime)
            {
                var targetRotation = Quaternion.LookRotation(targetOrientation);
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation, targetRotation, angularSpeed * Mathf.Rad2Deg * deltaTime);
            }

            public void Stop()
            {
                /* NOOP */
            }
        }
    }
}
