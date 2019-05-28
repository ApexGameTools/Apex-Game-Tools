/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace ApexEx.Examples.SceneSpecific.TargetPolling
{
    using Apex;
    using Apex.LoadBalancing;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Units;
    using UnityEngine;
    using Apex.WorldGeometry;

    public class StickyMove : MonoBehaviour, IHandleMessage<UnitNavigationEventMessage>
    {
        private Vector3 _destination;
        private IUnitFacade _unit;
        private ILoadBalancedHandle _pollHandle;
        private float _lastPollTime;

        public void MoveToSticky(Vector3 destination)
        {
            _destination = destination;
            if (_pollHandle != null)
            {
                _pollHandle.Stop();
            }

            GameServices.messageBus.Unsubscribe(this);
            GameServices.messageBus.Subscribe(this);

            _unit.MoveTo(destination, false);
        }

        private void Start()
        {
            _unit = this.GetUnitFacade();
        }

        private void StartPoll()
        {
            //Obviously this assumes there is only a single grid
            var grid = GridManager.instance.GetGrid(_unit.position);
            _lastPollTime = Time.time;

            _pollHandle = LoadBalancer.defaultBalancer.Execute((ignored) =>
            {
                if (grid.HasSectionsChangedSince(_unit.position, _lastPollTime))
                {
                    _unit.MoveTo(_destination, false);
                    _pollHandle = null;
                    return false;
                }

                return true;
            });
        }

        void IHandleMessage<UnitNavigationEventMessage>.Handle(UnitNavigationEventMessage message)
        {
            if (message.entity != _unit.gameObject)
            {
                return;
            }

            switch (message.eventCode)
            {
                case UnitNavigationEventMessage.Event.StoppedDestinationBlocked:
                case UnitNavigationEventMessage.Event.StoppedNoRouteExists:
                case UnitNavigationEventMessage.Event.Stuck:
                {
                    StartPoll();
                    break;
                }

                case UnitNavigationEventMessage.Event.DestinationReached:
                case UnitNavigationEventMessage.Event.StoppedByRequest:
                {
                    GameServices.messageBus.Unsubscribe(this);
                    break;
                }
            }
        }
    }
}
