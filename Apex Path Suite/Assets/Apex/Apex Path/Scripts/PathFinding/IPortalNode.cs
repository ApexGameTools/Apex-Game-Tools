/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using Apex.PathFinding.MoveCost;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Interface for portal nodes
    /// </summary>
    public interface IPortalNode : IPathNode
    {
        /// <summary>
        /// Gets the portal to which this node belongs.
        /// </summary>
        /// <value>
        /// The portal.
        /// </value>
        GridPortal portal { get; }

        /// <summary>
        /// Gets the partner portal cell.
        /// </summary>
        /// <value>
        /// The partner.
        /// </value>
        IPortalNode partner { get; }

        /// <summary>
        /// Gets the heuristic for a node in relation to this portal.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="goal">The goal.</param>
        /// <param name="moveCostProvider">The move cost provider.</param>
        /// <returns>The heuristic</returns>
        int GetHeuristic(IPathNode node, IPathNode goal, IMoveCost moveCostProvider);

        /// <summary>
        /// Gets the action cost.
        /// </summary>
        /// <param name="from">The node from which the action will start.</param>
        /// <param name="to">The node at which the action will end.</param>
        /// <param name="costProvider">The cost provider in use by the path finder.</param>
        /// <returns>The cost</returns>
        int GetCost(IPositioned from, IPositioned to, IMoveCost costProvider);

        /// <summary>
        /// Executes the portal move.
        /// </summary>
        /// <param name="unit">The unit that is entering the portal.</param>
        /// <param name="to">The destination at the other side of the portal.</param>
        /// <param name="callWhenComplete">The callback to call when the move is complete.</param>
        /// <returns>The grid of the destination.</returns>
        IGrid Execute(Transform unit, IPositioned to, Action callWhenComplete);
    }
}
