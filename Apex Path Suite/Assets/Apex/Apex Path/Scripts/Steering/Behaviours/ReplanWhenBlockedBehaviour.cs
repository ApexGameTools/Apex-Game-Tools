/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using Apex;
    using Apex.Common;
    using Apex.Messages;
    using Apex.Services;
    using Apex.Steering.Components;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A component that will force a replan of the route is is currently following if the destination it is moving towards gets blocked.
    /// It will then select the closest (to the original destination) non blocked cell as the destination instead.
    /// If using <see cref="SteerForPathComponent"/> it is preferred to use <see cref="SteerForPathReplanWhenBlocked"/> instead.
    /// </summary>
    [AddComponentMenu("")]
    [ApexComponent("Behaviours")]
    public class ReplanWhenBlockedBehaviour : ExtendedMonoBehaviour, IHandleMessage<UnitNavigationEventMessage>
    {
        /// <summary>
        /// The scan radius
        /// </summary>
        public int scanRadius = 2;

        private IUnitFacade _unit;

        private void Awake()
        {
            this.WarnIfMultipleInstances();

            _unit = this.GetUnitFacade();
            if (_unit == null)
            {
                Debug.LogError("ReplanWhenBlockedBehaviour requires a component that implements IMovable.");
                this.enabled = false;
            }
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
            if (message.eventCode == UnitNavigationEventMessage.Event.StoppedDestinationBlocked)
            {
                var grid = GridManager.instance.GetGrid(message.destination);
                if (grid == null)
                {
                    return;
                }

                var cell = grid.GetNearestWalkableCell(message.destination, this.transform.position, false, scanRadius, _unit);
                if (cell != null)
                {
                    message.isHandled = true;

                    _unit.MoveTo(cell.position, false);
                }
            }
        }

        private bool Walkable(Cell c)
        {
            return c.IsWalkableWithClearance(_unit);
        }

        private bool NotWalkable(Cell c)
        {
            return !c.IsWalkableWithClearance(_unit);
        }
    }
}
