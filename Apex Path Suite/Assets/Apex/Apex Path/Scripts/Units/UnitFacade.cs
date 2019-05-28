/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using System;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.WorldGeometry;
    using Messages;
    using UnityEngine;

    /// <summary>
    /// The default unit facade implementation. Represents the unit's properties and capabilities.
    /// </summary>
    public class UnitFacade : IUnitFacade
    {
        private int _formationIndex = -1;

        private IUnitProperties _props;
        private IMovable _movable;
        private IMovingObject _moving;
        private IDefineSpeed _speeder;
        private IPathFinderOptions _pathFinderOptions;
        private IPathNavigationOptions _pathNavOptions;

        /// <summary>
        /// Gets the unit's attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public AttributeMask attributes
        {
            get { return _props.attributes; }
        }

        /// <summary>
        /// Gets the ground offset, i..e the distance from the bottom of the unit's collider to the ground.
        /// </summary>
        /// <value>
        /// The ground offset.
        /// </value>
        public float groundOffset
        {
            get { return _props.groundOffset; }
        }

        /// <summary>
        /// Gets the offset between the unit's lower most point where it will touch the ground (touchdownPosition) and its position, typically the bottom of its collider to its position (y delta).
        /// </summary>
        /// <value>
        /// The position to ground offset.
        /// </value>
        public float baseToPositionOffset
        {
            get { return _props.baseToPositionOffset; }
        }

        /// <summary>
        /// Gets the unit's collider.
        /// </summary>
        /// <value>
        /// The collider.
        /// </value>
        public Collider collider
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The current path.
        /// </value>
        public Path currentPath
        {
            get { return _movable.currentPath; }
        }

        /// <summary>
        /// Gets the current way points.
        /// </summary>
        /// <value>
        /// The current way points.
        /// </value>
        public IIterable<Vector3> currentWaypoints
        {
            get { return _movable.currentWaypoints; }
        }

        /// <summary>
        /// Gets the unit's field of view in degrees.
        /// </summary>
        /// <value>
        /// The field of view.
        /// </value>
        public float fieldOfView
        {
            get { return _props.fieldOfView; }
        }

        /// <summary>
        /// Gets the final destination, which is either the last point in the <see cref="currentPath" /> or the last of the <see cref="currentWaypoints" /> if there are any.
        /// </summary>
        /// <value>
        /// The final destination.
        /// </value>
        public Vector3? finalDestination
        {
            get { return _movable.finalDestination; }
        }

        /// <summary>
        /// Gets the forward vector of the unit, i.e. the direction its nose is pointing (provided it has a nose).
        /// </summary>
        /// <value>
        /// The forward vector.
        /// </value>
        public Vector3 forward
        {
            get { return this.transform.forward; }
        }

        /// <summary>
        /// Gets or sets a point to look at. This does nothing by itself but enables e.g. <see cref="OrientationComponent" />s to respond to this.
        /// </summary>
        public Vector3? lookAt { get; set; }

        /// <summary>
        /// Gets the unit's game object.
        /// </summary>
        /// <value>
        /// The game object.
        /// </value>
        public GameObject gameObject
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height of the unit, i.e. from where it touches the ground to the top of its head (if it has one).
        /// </summary>
        /// <value>
        /// The height of the unit.
        /// </value>
        public float height
        {
            get { return _props.height; }
        }

        /// <summary>
        /// Gets the height navigation capability of the unit, i.e. how steep it can climb etc.
        /// </summary>
        /// <value>
        /// The height navigation capability.
        /// </value>
        public HeightNavigationCapabilities heightNavigationCapability
        {
            get { return _props.heightNavigationCapability; }
        }

        /// <summary>
        /// Gets a value indicating whether this object is grounded, i.e. not falling or otherwise raised above its natural base position.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grounded; otherwise, <c>false</c>.
        /// </value>
        public bool isGrounded
        {
            get { return _moving.isGrounded; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is movable, i.e. if the <see cref="IMovable" /> interface is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is movable; otherwise, <c>false</c>.
        /// </value>
        public bool isMovable
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this unit is alive. If the unit is not alive accessing other properties and methods will cause exceptions.
        /// This is set automatically, do not set this explicitly.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is alive; otherwise, <c>false</c>.
        /// </value>
        public bool isAlive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is selectable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selectable; otherwise, <c>false</c>.
        /// </value>
        public bool isSelectable
        {
            get { return _props.isSelectable; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this unit is selected. Only has an impact if <see cref="isSelectable" /> is true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        public bool isSelected
        {
            get { return _props.isSelected; }

            set { _props.isSelected = value; }
        }

        /// <summary>
        /// Gets the latest arrival event.
        /// </summary>
        /// <value>
        /// The latest arrival event.
        /// </value>
        public UnitNavigationEventMessage.Event lastNavigationEvent
        {
            get { return _movable.lastNavigationEvent; }
        }

        /// <summary>
        /// Gets the maximum acceleration (m / s^2), i.e. how fast can the unit reach its desired speed.
        /// </summary>
        /// <value>
        /// The maximum acceleration.
        /// </value>
        public float maxAcceleration
        {
            get { return _speeder.maxAcceleration; }
        }

        /// <summary>
        /// Gets the maximum angular acceleration (rads / s^2), i.e. how fast can the unit reach its desired turn speed.
        /// </summary>
        /// <value>
        /// The maximum angular acceleration.
        /// </value>
        public float maxAngularAcceleration
        {
            get { return _speeder.maxAngularAcceleration; }
        }

        /// <summary>
        /// Gets the maximum deceleration (m / s^2), i.e. how fast can the unit slow down.
        /// </summary>
        /// <value>
        /// The maximum deceleration.
        /// </value>
        public float maxDeceleration
        {
            get { return _speeder.maxDeceleration; }
        }

        /// <summary>
        /// Gets the maximum angular speed (rads / s), i.e. how fast can the unit turn.
        /// </summary>
        /// <value>
        /// The maximum angular speed.
        /// </value>
        public float maximumAngularSpeed
        {
            get { return _speeder.maximumAngularSpeed; }
        }

        /// <summary>
        /// Gets the maximum speed of the unit.
        /// </summary>
        /// <value>
        /// The maximum speed.
        /// </value>
        public float maximumSpeed
        {
            get { return _speeder.maximumSpeed; }
        }

        /// <summary>
        /// Gets the minimum speed of the unit. Any speed below this value will mean a stop.
        /// </summary>
        /// <value>
        /// The minimum speed.
        /// </value>
        public float minimumSpeed
        {
            get { return _speeder.minimumSpeed; }
        }

        /// <summary>
        /// Gets the position of the next node along the path currently being moved towards.
        /// </summary>
        /// <value>
        /// The next node position.
        /// </value>
        public Vector3? nextNodePosition
        {
            get { return _movable.nextNodePosition; }
        }

        /// <summary>
        /// Gets the path finder options to use for issuing path requests.
        /// </summary>
        /// <value>
        /// The path finder options.
        /// </value>
        public IPathFinderOptions pathFinderOptions
        {
            get { return _pathFinderOptions; }
        }

        /// <summary>
        /// Gets the path navigation options.
        /// </summary>
        /// <value>
        /// The path navigation options.
        /// </value>
        public IPathNavigationOptions pathNavigationOptions
        {
            get { return _pathNavOptions; }
        }

        /// <summary>
        /// Gets the position of the unit.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 position
        {
            get { return this.transform.position; }
        }

        /// <summary>
        /// Gets the radius of the unit
        /// </summary>
        /// <value>
        /// The radius of the unit
        /// </value>
        public float radius
        {
            get { return _props.radius; }
        }

        /// <summary>
        /// Gets the position where the unit touches the ground (if it is grounded). This is its position offset by baseToPositionOffset
        /// </summary>
        /// <value>
        /// The touchdown position.
        /// </value>
        public Vector3 basePosition
        {
            get { return _props.basePosition; }
        }

        /// <summary>
        /// Gets the unit's transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        public Transform transform
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the unit's group. Setting this manually is not recommended.
        /// </summary>
        /// <value>
        /// The transient group.
        /// </value>
        public TransientGroup<IUnitFacade> transientGroup
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the velocity of the unit. This represents the movement force applied to the unit. Also see <see cref="actualVelocity" />.
        /// </summary>
        /// <value>
        /// The velocity.
        /// </value>
        public Vector3 velocity
        {
            get { return _moving.velocity; }
        }

        /// <summary>
        /// Gets the actual velocity of the unit. This may differ from <see cref="velocity"/> in certain scenarios, e.g. during collisions, if being moved by other means etc.
        /// </summary>
        /// <value>
        /// The actual velocity.
        /// </value>
        public Vector3 actualVelocity
        {
            get { return _moving.actualVelocity; }
        }

        /// <summary>
        /// Gets or sets the formation index, i.e. the place in the group formation.
        /// </summary>
        /// <value>
        /// The index of the formation.
        /// </value>
        public int formationIndex
        {
            get
            {
                return _formationIndex;
            }

            set
            {
                _formationIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the unit's desired formation position.
        /// </summary>
        /// <value>
        /// The formation position.
        /// </value>
        public IPositioned formationPos
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has arrived at its formation position or the vector field's final destination, depending on whether there is a valid formation.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has arrived at its position; otherwise, <c>false</c>.
        /// </value>
        public bool hasArrivedAtDestination
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the determination factor used to evaluate whether this unit separates or avoids other units. The higher the determination, the less avoidance/separation.
        /// </summary>
        /// <value>
        /// The determination.
        /// </value>
        public int determination
        {
            get { return _props.determination; }
            set { _props.determination = value; }
        }

        /// <summary>
        /// Disables movement orders, i.e. calls to <see cref="MoveTo" /> will be ignored until <see cref="EnableMovementOrders" /> is called.
        /// </summary>
        public void DisableMovementOrders()
        {
            _movable.DisableMovementOrders();
        }

        /// <summary>
        /// Enables the movement orders following a call to <see cref="DisableMovementOrders" />.
        /// </summary>
        public void EnableMovementOrders()
        {
            _movable.EnableMovementOrders();
        }

        /// <summary>
        /// Sets the preferred speed of the unit.
        /// </summary>
        /// <param name="speed">The speed.</param>
        public void SetPreferredSpeed(float speed)
        {
            _speeder.SetPreferredSpeed(speed);
        }

        /// <summary>
        /// Gets the preferred speed of the unit.
        /// </summary>
        /// <param name="currentMovementDirection">The current movement direction.</param>
        /// <returns>
        /// The preferred speed
        /// </returns>
        public float GetPreferredSpeed(Vector3 currentMovementDirection)
        {
            return _speeder.GetPreferredSpeed(currentMovementDirection);
        }

        /// <summary>
        /// Marks the unit as pending for selection. This is used to indicate a selection is progress, before the actual selection occurs.
        /// </summary>
        /// <param name="pending">if set to <c>true</c> the unit is pending for selection otherwise it is not.</param>
        public void MarkSelectPending(bool pending)
        {
            _props.MarkSelectPending(pending);
        }

        /// <summary>
        /// Asks the object to move along the specified path. Replanning is done by the path finder.
        /// </summary>
        /// <param name="path">The path.</param>
        public void MoveAlong(Path path)
        {
            _movable.MoveAlong(path);
        }

        /// <summary>
        /// Asks the object to move along the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        public void MoveAlong(Path path, ReplanCallback onReplan)
        {
            _movable.MoveAlong(path, onReplan);
        }

        /// <summary>
        /// Asks the object to move to the specified position
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        public void MoveTo(Vector3 position, bool append)
        {
            _movable.MoveTo(position, append);
        }

        /// <summary>
        /// Recalculates the base position and unit height. Call this if the unit's size changes at runtime
        /// </summary>
        public void RecalculateBasePosition()
        {
            _props.RecalculateBasePosition();
        }

        /// <summary>
        /// Resumes movements after a <see cref="Wait" />.
        /// </summary>
        public void Resume()
        {
            _moving.Resume();
        }

        /// <summary>
        /// Signal that the unit has stopped.
        /// </summary>
        public void SignalStop()
        {
            _speeder.SignalStop();
        }

        /// <summary>
        /// Stops the unit's movement.
        /// </summary>
        public void Stop()
        {
            _moving.Stop();
        }

        /// <summary>
        /// Waits the specified seconds before continuing the move.
        /// </summary>
        /// <param name="seconds">The seconds to wait or null to wait until explicitly <see cref="Resume" />d.</param>
        public void Wait(float? seconds)
        {
            _moving.Wait(seconds);
        }

        void IDefineSpeed.CloneFrom(IDefineSpeed speedComponent)
        {
            _speeder.CloneFrom(speedComponent);
        }

        /// <summary>
        /// Initializes the Unit Facade.
        /// </summary>
        /// <param name="unitObject">The unit game object.</param>
        public virtual void Initialize(GameObject unitObject)
        {
            _props = unitObject.As<IUnitProperties>(false, true);
            _movable = unitObject.As<IMovable>(false, false);
            _moving = unitObject.As<IMovingObject>(false, true);
            _speeder = unitObject.As<IDefineSpeed>(false, true);
            _pathFinderOptions = unitObject.As<IPathFinderOptions>(false, false) ?? new PathFinderOptions();
            _pathNavOptions = unitObject.As<IPathNavigationOptions>(false, false) ?? new PathNavigationOptions();

            this.isMovable = _movable != null;
            if (!this.isMovable)
            {
                _movable = new MovableDummy(unitObject.name);
            }

            this.gameObject = unitObject;
            this.transform = unitObject.transform;
            this.collider = unitObject.GetComponent<Collider>();

            this.isAlive = true;
            this.hasArrivedAtDestination = true;
        }

        private class MovableDummy : IMovable
        {
            private string _gameObjectName;

            public MovableDummy(string gameObjectName)
            {
                _gameObjectName = gameObjectName;
            }

            public Path currentPath
            {
                get
                {
                    ThrowException();
                    return null;
                }
            }

            public IIterable<Vector3> currentWaypoints
            {
                get
                {
                    ThrowException();
                    return null;
                }
            }

            public Vector3? finalDestination
            {
                get
                {
                    ThrowException();
                    return null;
                }
            }

            public Vector3? nextNodePosition
            {
                get
                {
                    ThrowException();
                    return null;
                }
            }

            public UnitNavigationEventMessage.Event lastNavigationEvent
            {
                get
                {
                    ThrowException();
                    return UnitNavigationEventMessage.Event.None;
                }
            }

            public void MoveTo(Vector3 position, bool append)
            {
                ThrowException();
            }

            public void MoveAlong(Path path)
            {
                ThrowException();
            }

            public void MoveAlong(Path path, ReplanCallback onReplan)
            {
                ThrowException();
            }

            public void Wait(float? seconds)
            {
                ThrowException();
            }

            public void Resume()
            {
                ThrowException();
            }

            public void EnableMovementOrders()
            {
                ThrowException();
            }

            public void DisableMovementOrders()
            {
                ThrowException();
            }

            private void ThrowException()
            {
                throw new MissingComponentException(string.Format("Game object {0} does not have a component of type IMovable, which is required to call IMovable methods and properties.", _gameObjectName));
            }
        }
    }
}