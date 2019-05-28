/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using Apex;
    using Apex.Messages;
    using Apex.Services;
    using UnityEngine;

    /// <summary>
    /// A component that will force a replan of the route if it gets stuck along the way.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Replan When Stuck", 1030)]
    [ApexComponent("Behaviours")]
    public class ReplanWhenStuckBehaviour : ExtendedMonoBehaviour, IHandleMessage<UnitNavigationEventMessage>
    {
        private int _currentRetries;

        /// <summary>
        /// The maximum retries
        /// </summary>
        public int maxRetries = 3;

        private void Awake()
        {
            this.WarnIfMultipleInstances();
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            GameServices.messageBus.Subscribe(this);
        }

        private void OnDisable()
        {
            GameServices.messageBus.Unsubscribe(this);
        }

        void IHandleMessage<UnitNavigationEventMessage>.Handle(UnitNavigationEventMessage message)
        {
            if (message.entity != this.gameObject)
            {
                return;
            }

            if (message.eventCode == UnitNavigationEventMessage.Event.Stuck)
            {
                if (_currentRetries++ > this.maxRetries)
                {
                    return;
                }

                var unit = this.GetUnitFacade();
                if (unit == null)
                {
                    return;
                }

                unit.MoveTo(message.destination, false);
                if (message.pendingWaypoints != null)
                {
                    var pending = message.pendingWaypoints;
                    var count = pending.Length;
                    for (int i = 0; i < count; i++)
                    {
                        unit.MoveTo(pending[i], true);
                    }
                }
            }
            else
            {
                _currentRetries = 0;
            }
        }
    }
}
