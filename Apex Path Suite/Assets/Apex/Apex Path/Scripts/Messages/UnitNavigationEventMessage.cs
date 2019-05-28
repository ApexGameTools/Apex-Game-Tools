/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Messages
{
    using UnityEngine;

    /// <summary>
    /// A message for use with the <see cref="Apex.Services.IMessageBus"/> to signal navigation events.
    /// </summary>
    public class UnitNavigationEventMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitNavigationEventMessage"/> class.
        /// </summary>
        /// <param name="entity">The entity that this message concerns.</param>
        public UnitNavigationEventMessage(GameObject entity)
            : this(entity, Event.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitNavigationEventMessage"/> class.
        /// </summary>
        /// <param name="entity">The entity that this message concerns.</param>
        /// <param name="e">The event.</param>
        public UnitNavigationEventMessage(GameObject entity, Event e)
        {
            this.entity = entity;
            this.eventCode = e;
        }

        /// <summary>
        /// The various navigation events
        /// </summary>
        public enum Event
        {
            /// <summary>
            /// No event
            /// </summary>
            None,

            /// <summary>
            /// A way point was reached
            /// </summary>
            WaypointReached,

            /// <summary>
            /// The destination was reached
            /// </summary>
            DestinationReached,

            /// <summary>
            /// The unit stopped as no route exists to its proposed destination
            /// </summary>
            StoppedNoRouteExists,

            /// <summary>
            /// The unit stopped as its destination is blocked
            /// </summary>
            StoppedDestinationBlocked,

            /// <summary>
            /// The unit stopped as its path request decayed
            /// </summary>
            StoppedRequestDecayed,

            /// <summary>
            /// The unit got stuck
            /// </summary>
            Stuck,

            /// <summary>
            /// A node along the path was reached
            /// </summary>
            NodeReached,

            /// <summary>
            /// The unit stopped since it is outside the grid
            /// </summary>
            StoppedUnitOutsideGrid,

            /// <summary>
            /// The unit was stopped by request
            /// </summary>
            StoppedByRequest
        }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        /// <value>
        /// The event code.
        /// </value>
        public Event eventCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the destination of the entity.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public Vector3 destination
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pending way points.
        /// </summary>
        /// <value>
        /// The pending way points.
        /// </value>
        public Vector3[] pendingWaypoints
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the entity that this message concerns.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public GameObject entity
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this message is handled. It allows consumers to indicate that they have handled the message, i.e. taken some action in response to it.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is handled; otherwise, <c>false</c>.
        /// </value>
        public bool isHandled
        {
            get;
            set;
        }
    }
}
