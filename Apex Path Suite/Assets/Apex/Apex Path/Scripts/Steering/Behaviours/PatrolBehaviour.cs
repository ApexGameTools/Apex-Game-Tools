/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using System.Collections;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Steering.Props;
    using UnityEngine;

    /// <summary>
    /// A steering behaviour that will make the unit to which it is attached, patrol a given <see cref="PatrolRoute"/>
    /// </summary>
    [AddComponentMenu("Apex/Legacy/Navigation/Patrol (OLD)", 1020)]
    [ApexComponent("Behaviours")]
    public class PatrolBehaviour : ExtendedMonoBehaviour, IHandleMessage<UnitNavigationEventMessage>
    {
        /// <summary>
        /// The route to patrol
        /// </summary>
        public PatrolRoute route;

        /// <summary>
        /// The time in seconds that the unit will linger at each patrol point before moving on.
        /// </summary>
        public float lingerAtNodesForSeconds = 0.0f;

        /// <summary>
        /// Whether to patrol the route in reverse direction.
        /// </summary>
        public bool reverseRoute;

        /// <summary>
        /// If set to true, the patrol points are visited in a random manner rather than sequentially in order.
        /// </summary>
        public bool randomize;

        private IMovable _mover;
        private int _currentPatrolPointIdx;

        private void Awake()
        {
            this.WarnIfMultipleInstances();

            _mover = this.As<IMovable>();
            if (_mover == null)
            {
                Debug.LogError("PatrolBehaviour requires a component that implements IMovable.");
                this.enabled = false;
            }
        }

        /// <summary>
        /// Called on Start
        /// </summary>
        protected override void Start()
        {
            if (this.route == null || this.route.patrolPoints.Length < 2)
            {
                Debug.LogWarning("A patrol route with at least two points is required to patrol.");
                this.enabled = false;
                return;
            }

            _currentPatrolPointIdx = -1;

            base.Start();
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            GameServices.messageBus.Subscribe(this);

            MoveNext(false);
            if (this.lingerAtNodesForSeconds == 0.0f)
            {
                MoveNext(true);
            }
        }

        private void OnDisable()
        {
            GameServices.messageBus.Unsubscribe(this);
        }

        void IHandleMessage<UnitNavigationEventMessage>.Handle(UnitNavigationEventMessage message)
        {
            if (message.entity != this.gameObject || message.isHandled)
            {
                return;
            }

            if (message.eventCode == UnitNavigationEventMessage.Event.WaypointReached)
            {
                message.isHandled = true;

                MoveNext(true);
            }
            else if (message.eventCode == UnitNavigationEventMessage.Event.DestinationReached)
            {
                message.isHandled = true;

                StartCoroutine(DelayedMove());
            }
        }

        private IEnumerator DelayedMove()
        {
            yield return new WaitForSeconds(this.lingerAtNodesForSeconds);
            MoveNext(false);
        }

        private void MoveNext(bool append)
        {
            var points = this.route.patrolPoints;

            if (this.randomize)
            {
                var tmp = _currentPatrolPointIdx;
                while (tmp == _currentPatrolPointIdx)
                {
                    _currentPatrolPointIdx = Random.Range(0, points.Length - 1);
                }
            }
            else
            {
                _currentPatrolPointIdx = ++_currentPatrolPointIdx % points.Length;
            }

            int idx = _currentPatrolPointIdx;
            if (this.reverseRoute)
            {
                idx = (points.Length - 1) - _currentPatrolPointIdx;
            }

            _mover.MoveTo(points[idx].position, append);
        }
    }
}
