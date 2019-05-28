/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Steering.Components
{
    using System;
    using Apex.DataStructures;
    using Apex.Messages;
    using Apex.PathFinding;
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to seek towards a given target.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Steer for Point", 1024)]
    public class SteerForPointComponent : ArrivalBase, IMovable
    {
        private Vector3 _point;

        public Path currentPath
        {
            get { return null; }
        }

        public IIterable<Vector3> currentWaypoints
        {
            get { return null; }
        }

        public Vector3? finalDestination
        {
            get { return _point; }
        }

        public Vector3? nextNodePosition
        {
            get { return _point; }
        }

        public UnitNavigationEventMessage.Event lastNavigationEvent
        {
            get { return UnitNavigationEventMessage.Event.None; }
        }

        public void MoveTo(Vector3 position, bool append)
        {
            _point = position;
        }

        public void MoveAlong(PathFinding.Path path)
        {
            throw new System.NotImplementedException();
        }

        public void MoveAlong(PathFinding.Path path, PathFinding.ReplanCallback onReplan)
        {
            throw new System.NotImplementedException();
        }

        public void EnableMovementOrders()
        {
            throw new System.NotImplementedException();
        }

        public void DisableMovementOrders()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            var acc = Arrive(_point, input);
            if (this.hasArrived)
            {
                return;
            }

            output.desiredAcceleration = acc;
        }

        /// <summary>
        /// Stop the unit.
        /// </summary>
        public override void Stop()
        {
            _point = this.transform.position;
        }
    }
}
