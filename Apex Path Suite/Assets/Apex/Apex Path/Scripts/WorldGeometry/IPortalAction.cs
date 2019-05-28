/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using Apex.PathFinding;
    using Apex.PathFinding.MoveCost;
    using UnityEngine;

    /// <summary>
    /// Interface for portal actions, i.e. actions that transport a unit from one side of a portal to the other.
    /// </summary>
    public interface IPortalAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="unit">The unit that has entered the portal.</param>
        /// <param name="from">The portal cell that was entered.</param>
        /// <param name="to">The destination at the other side of the portal.</param>
        /// <param name="callWhenComplete">The callback to call when the move is complete.</param>
        void Execute(Transform unit, PortalCell from, IPositioned to, Action callWhenComplete);

        /// <summary>
        /// Gets the action cost.
        /// </summary>
        /// <param name="from">The node from which the action will start.</param>
        /// <param name="to">The node at which the action will end.</param>
        /// <param name="costProvider">The cost provider in use by the path finder.</param>
        /// <returns>The cost</returns>
        int GetActionCost(IPositioned from, IPositioned to, IMoveCost costProvider);
    }
}
