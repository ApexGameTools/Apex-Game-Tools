/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using Apex.DataStructures;
    using Apex.Messages;
    using Apex.PathFinding;
    using UnityEngine;

    /// <summary>
    /// Interface for objects that can move
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The current path.
        /// </value>
        Path currentPath { get; }

        /// <summary>
        /// Gets the current way points.
        /// </summary>
        /// <value>
        /// The current way points.
        /// </value>
        IIterable<Vector3> currentWaypoints { get; }

        /// <summary>
        /// Gets the final destination, which is either the last point in the <see cref="currentPath"/> or the last of the <see cref="currentWaypoints"/> if there are any.
        /// </summary>
        /// <value>
        /// The final destination.
        /// </value>
        Vector3? finalDestination { get; }

        /// <summary>
        /// Gets the position of the next node along the path currently being moved towards.
        /// </summary>
        /// <value>
        /// The next node position.
        /// </value>
        Vector3? nextNodePosition { get; }

        /// <summary>
        /// Gets the latest arrival event.
        /// </summary>
        /// <value>
        /// The latest arrival event.
        /// </value>
        UnitNavigationEventMessage.Event lastNavigationEvent { get; }

        /// <summary>
        /// Asks the object to move to the specified position
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="append">if set to <c>true</c> the destination is added as a way point.</param>
        void MoveTo(Vector3 position, bool append);

        /// <summary>
        /// Asks the object to move along the specified path. Replanning is done by the path finder.
        /// </summary>
        /// <param name="path">The path.</param>
        void MoveAlong(Path path);

        /// <summary>
        /// Asks the object to move along the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="onReplan">The callback to call when replanning is needed.</param>
        void MoveAlong(Path path, ReplanCallback onReplan);
        
        /// <summary>
        /// Enables the movement orders following a call to <see cref="DisableMovementOrders"/>.
        /// </summary>
        void EnableMovementOrders();

        /// <summary>
        /// Disables movement orders, i.e. calls to <see cref="MoveTo"/> will be ignored until <see cref="EnableMovementOrders"/> is called.
        /// </summary>
        void DisableMovementOrders();
    }
}
