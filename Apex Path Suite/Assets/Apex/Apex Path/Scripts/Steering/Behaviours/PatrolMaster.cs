/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using System.Collections.Generic;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Manager class that keeps track of patrolling units.
    /// </summary>
    public static class PatrolMaster
    {
        private static readonly Dictionary<GameObject, PatrolClient> _clients = new Dictionary<GameObject, PatrolClient>();
        private static readonly Dictionary<GameObject, PatrolClient> _pausedClients = new Dictionary<GameObject, PatrolClient>();
        private static Handler _handler;

        /// <summary>
        /// Orders the unit to patrol a given patrol route.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="patrolPoints">The patrol route.</param>
        /// <param name="randomize">Whether to randomize the points of the route.</param>
        /// <param name="reverse">Whether to reverse the route."</param>
        /// <param name="lingerForSeconds">How long to wait at each point before moving on to the next.</param>
        public static void Patrol(this IUnitFacade unit, Vector3[] patrolPoints, bool randomize = false, bool reverse = false, float lingerForSeconds = 0f)
        {
            if (_handler == null)
            {
                _handler = new Handler();
            }

            var client = new PatrolClient
            {
                route = patrolPoints,
                randomize = randomize,
                reverseRoute = reverse,
                lingerForSeconds = lingerForSeconds,
                unit = unit
            };

            _clients[unit.gameObject] = client;

            client.Start();
        }

        /// <summary>
        /// Stops the unit's patrol.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public static void StopPatrol(this IUnitFacade unit)
        {
            PatrolClient client;
            if (!_clients.TryGetValue(unit.gameObject, out client))
            {
                if (!_pausedClients.TryGetValue(unit.gameObject, out client))
                {
                    return;
                }
            }

            _clients.Remove(unit.gameObject);
            _pausedClients.Remove(unit.gameObject);

            if (_clients.Count == 0)
            {
                _handler.Stop();
                _handler = null;
            }

            if (unit.isAlive)
            {
                client.Stop();
            }
        }

        /// <summary>
        /// Pauses the patrol.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public static void PausePatrol(this IUnitFacade unit)
        {
            PatrolClient client;
            if (!_clients.TryGetValue(unit.gameObject, out client))
            {
                return;
            }

            _clients.Remove(unit.gameObject);

            if (unit.isAlive)
            {
                _pausedClients[unit.gameObject] = client;

                client.Stop();
            }
        }

        /// <summary>
        /// Resumes the patrol.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns><c>true</c> if the patrol could be resumed; otherwise <c>false</c></returns>
        public static bool ResumePatrol(this IUnitFacade unit)
        {
            PatrolClient client;
            if (!_pausedClients.TryGetValue(unit.gameObject, out client))
            {
                return false;
            }

            _pausedClients.Remove(unit.gameObject);

            if (unit.isAlive)
            {
                _clients[unit.gameObject] = client;

                client.Resume();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the unit is currently patrolling. Units who are paused will still return true here.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns><c>true</c> if the unit is associated with a patrol; otherwise <c>false</c></returns>
        public static bool IsOnPatrol(this IUnitFacade unit)
        {
            return _clients.ContainsKey(unit.gameObject) || _pausedClients.ContainsKey(unit.gameObject);
        }

        private static bool Continue(GameObject go)
        {
            PatrolClient client;
            if (!_clients.TryGetValue(go, out client))
            {
                return false;
            }

            if (client.lingerForSeconds > 0f)
            {
                LoadBalancer.defaultBalancer.ExecuteOnce(client.Continue, client.lingerForSeconds);
            }
            else
            {
                client.Continue();
            }

            return true;
        }

        private static bool Stop(GameObject go)
        {
            var unit = go.GetUnitFacade(false);
            if (unit == null)
            {
                return false;
            }

            if (!unit.IsOnPatrol())
            {
                return false;
            }

            unit.StopPatrol();
            return true;
        }

        private class PatrolClient
        {
            private int _currentIndex;
            private int _nextIndex;
            private bool _stopped;

            internal Vector3[] route;
            internal IUnitFacade unit;
            internal bool randomize;
            internal bool reverseRoute;
            internal float lingerForSeconds;

            internal void Start()
            {
                _nextIndex = this.reverseRoute ? this.route.Length : -1;
                IncrementNext();

                _currentIndex = _nextIndex;
                IncrementNext();

                Resume();
            }

            internal void Stop()
            {
                this.unit.Stop();
                _stopped = true;
            }

            internal void Resume()
            {
                _stopped = false;

                this.unit.MoveTo(route[_currentIndex], false);

                if (this.lingerForSeconds == 0f)
                {
                    this.unit.MoveTo(route[_nextIndex], true);
                }
            }

            internal void Continue()
            {
                if (_stopped)
                {
                    return;
                }
                else if (!this.unit.isAlive)
                {
                    PatrolMaster.StopPatrol(this.unit);
                    return;
                }

                _currentIndex = _nextIndex;
                IncrementNext();

                if (this.lingerForSeconds == 0f)
                {
                    this.unit.MoveTo(route[_nextIndex], true);
                }
                else
                {
                    this.unit.MoveTo(route[_currentIndex], false);
                }
            }

            private void IncrementNext()
            {
                if (this.randomize)
                {
                    var tmp = _nextIndex;
                    while (tmp == _nextIndex)
                    {
                        _nextIndex = Random.Range(0, route.Length);
                    }
                }
                else if (this.reverseRoute)
                {
                    _nextIndex = (--_nextIndex + route.Length) % route.Length;
                }
                else
                {
                    _nextIndex = ++_nextIndex % route.Length;
                }
            }
        }

        private class Handler : IHandleMessage<UnitNavigationEventMessage>
        {
            public Handler()
            {
                GameServices.messageBus.Subscribe(this);
            }

            public void Stop()
            {
                GameServices.messageBus.Unsubscribe(this);
            }

            void IHandleMessage<UnitNavigationEventMessage>.Handle(UnitNavigationEventMessage message)
            {
                if (message.isHandled)
                {
                    return;
                }

                switch (message.eventCode)
                {
                    case UnitNavigationEventMessage.Event.WaypointReached:
                    case UnitNavigationEventMessage.Event.DestinationReached:
                    {
                        message.isHandled = PatrolMaster.Continue(message.entity);
                        break;
                    }

                    case UnitNavigationEventMessage.Event.StoppedNoRouteExists:
                    case UnitNavigationEventMessage.Event.StoppedDestinationBlocked:
                    case UnitNavigationEventMessage.Event.Stuck:
                    {
                        message.isHandled = PatrolMaster.Stop(message.entity);
                        break;
                    }
                }
            }
        }
    }
}
