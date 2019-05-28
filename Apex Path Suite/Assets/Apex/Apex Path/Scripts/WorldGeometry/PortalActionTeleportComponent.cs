/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using Apex.PathFinding.MoveCost;
    using Apex.Services;
    using Apex.Steering;
    using UnityEngine;

    /// <summary>
    /// Simple portal action that teleports units from one end to the other.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Portals/Action Teleport", 1018)]
    [ApexComponent("Game World")]
    public class PortalActionTeleportComponent : MonoBehaviour, IPortalAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="unit">The unit that has entered the portal.</param>
        /// <param name="from">The portal cell that was entered.</param>
        /// <param name="to">The destination at the other side of the portal.</param>
        /// <param name="callWhenComplete">The callback to call when the move is complete.</param>
        public void Execute(Transform unit, PortalCell from, IPositioned to, Action callWhenComplete)
        {
            var heightSampler = GameServices.heightStrategy.heightSampler;
            float fromHeight;
            if (!heightSampler.TrySampleHeight(unit.position, out fromHeight))
            {
                fromHeight = to.position.y;
            }

            var heightAdjustment = to.position.y - fromHeight;
            unit.position = new Vector3(to.position.x, unit.position.y + heightAdjustment, to.position.z);

            callWhenComplete();
        }

        /// <summary>
        /// Gets the action cost.
        /// </summary>
        /// <param name="from">The node from which the action will start.</param>
        /// <param name="to">The node at which the action will end.</param>
        /// <param name="costProvider">The cost provider in use by the path finder.</param>
        /// <returns></returns>
        public int GetActionCost(IPositioned from, IPositioned to, IMoveCost costProvider)
        {
            return 0;
        }
    }
}
