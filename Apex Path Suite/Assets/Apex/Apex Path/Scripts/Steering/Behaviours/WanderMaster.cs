/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using System.Collections.Generic;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Manager class that keeps track of wandering clients.
    /// </summary>
    public static class WanderMaster
    {
        /// <summary>
        /// If unable to find a spot to wander to after having tried <see cref="bailAfterFailedAttempts"/> no more attempts will be made.
        /// </summary>
        public static int bailAfterFailedAttempts = 100;

        private static readonly Dictionary<GameObject, WanderClient> _clients = new Dictionary<GameObject, WanderClient>();
        private static Handler _handler;

        /// <summary>
        /// Makes the specified unit wander around at random.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="radius">The radius within which to wander (seen from the unit center).</param>
        /// <param name="minimumDistance">The minimum distance to wander, each time a new destination is picked.</param>
        /// <param name="lingerForSeconds">How many seconds to linger at each destination before moving on.</param>
        public static void Wander(this IUnitFacade unit, float radius, float minimumDistance, float lingerForSeconds)
        {
            if (_handler == null)
            {
                _handler = new Handler();
            }

            var client = new WanderClient
            {
                radius = radius,
                minimumDistance = minimumDistance,
                lingerForSeconds = lingerForSeconds,
                unit = unit
            };

            _clients.Add(unit.gameObject, client);

            Start(client);
        }

        /// <summary>
        /// Stops the unit from wandering.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public static void StopWander(this IUnitFacade unit)
        {
            _clients.Remove(unit.gameObject);

            if (_clients.Count == 0 && _handler != null)
            {
                _handler.Stop();
                _handler = null;
            }

            if (unit.isAlive)
            {
                unit.Stop();
            }
        }

        private static bool DelayedMove(GameObject go)
        {
            WanderClient client;
            if (!_clients.TryGetValue(go, out client))
            {
                return false;
            }

            LoadBalancer.defaultBalancer.ExecuteOnce(client.MoveNext, client.lingerForSeconds);
            return true;
        }

        private static bool Start(GameObject go)
        {
            WanderClient client;
            if (!_clients.TryGetValue(go, out client))
            {
                return false;
            }

            Start(client);
            return true;
        }

        private static void Start(WanderClient client)
        {
            MoveNext(client.unit.gameObject, false);
            if (client.lingerForSeconds == 0.0f)
            {
                MoveNext(client.unit.gameObject, true);
            }
        }

        private static bool MoveNext(GameObject go, bool append)
        {
            WanderClient client;
            if (!_clients.TryGetValue(go, out client))
            {
                return false;
            }

            MoveNext(client, append);
            return true;
        }

        private static void MoveNext(WanderClient client, bool append)
        {
            if (!client.unit.isAlive)
            {
                StopWander(client.unit);
                return;
            }

            Vector3 pos = Vector3.zero;
            int attempts = 0;

            while (attempts < bailAfterFailedAttempts)
            {
                pos = client.unit.position + (Random.insideUnitSphere.normalized.OnlyXZ() * Random.Range(client.minimumDistance, client.radius));

                var grid = GridManager.instance.GetGrid(pos);
                if (grid != null)
                {
                    var cell = grid.GetCell(pos, true);
                    if (cell.IsWalkableWithClearance(client.unit))
                    {
                        client.unit.MoveTo(cell.position, append);
                        return;
                    }
                }

                attempts++;
            }

            //If we didn't find a wander point, stop wandering.
            StopWander(client.unit);
        }

        private class WanderClient
        {
            /// <summary>
            /// The radius from the starting position within which to wander
            /// </summary>
            internal float radius = 10.0f;

            /// <summary>
            /// The minimum distance of a wander route
            /// </summary>
            internal float minimumDistance = 4.0f;

            /// <summary>
            /// The time in seconds that the unit will linger after each wander route before moving on.
            /// </summary>
            internal float lingerForSeconds = 0.0f;

            /// <summary>
            /// The unit
            /// </summary>
            internal IUnitFacade unit;

            internal void MoveNext()
            {
                WanderMaster.MoveNext(this, false);
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
                    {
                        message.isHandled = WanderMaster.MoveNext(message.entity, true);
                        break;
                    }

                    case UnitNavigationEventMessage.Event.DestinationReached:
                    {
                        message.isHandled = WanderMaster.DelayedMove(message.entity);
                        break;
                    }

                    case UnitNavigationEventMessage.Event.StoppedNoRouteExists:
                    case UnitNavigationEventMessage.Event.StoppedDestinationBlocked:
                    case UnitNavigationEventMessage.Event.Stuck:
                    {
                        message.isHandled = WanderMaster.Start(message.entity);
                        break;
                    }
                }
            }
        }
    }
}
