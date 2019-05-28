/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.Units;
    using Apex.WorldGeometry;

    /// <summary>
    /// Interface for path nodes, i.e. cells that make up the building blocks of a path.
    /// </summary>
    public interface IPathNode : IGridCell, IPositioned
    {
        /// <summary>
        /// Gets or sets the current cost to reach this node, i.e. the g in the f = g + h a start cost function.
        /// </summary>
        /// <value>
        /// The g.
        /// </value>
        int g { get; set; }

        /// <summary>
        /// Gets or sets the future cost (heuristic) from this node to the goal, i.e. the h in the f = g + h a start cost function.
        /// </summary>
        /// <value>
        /// The f.
        /// </value>
        int h { get; set; }

        /// <summary>
        /// Gets or sets the cost of this node, i.e. the sum of known cost g and future estimated cost h, f = g + h .
        /// </summary>
        /// <value>
        /// The f.
        /// </value>
        int f { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is closed (closed set).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is closed; otherwise, <c>false</c>.
        /// </value>
        bool isClosed { get; set; }

        /// <summary>
        /// Gets or sets the predecessor of this node.
        /// </summary>
        /// <value>
        /// The predecessor.
        /// </value>
        IPathNode predecessor { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has a virtual neighbour.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has a virtual neighbour; otherwise, <c>false</c>.
        /// </value>
        bool hasVirtualNeighbour { get; }

        /// <summary>
        /// Gets the walkable neighbours.
        /// </summary>
        /// <param name="neighbours">An array to fill with neighbours.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <param name="cornerCuttingAllowed">if set to <c>true</c> corner cutting is allowed, which controls which diagonal moves are deemed valid.</param>
        /// <param name="preventDiagonalMoves">controls whether diagonal moves are allowed, i.e. does a cell have 4 or 8 potential neighbours.</param>
        void GetWalkableNeighbours(DynamicArray<IPathNode> neighbours, IUnitProperties unitProps, bool cornerCuttingAllowed, bool preventDiagonalMoves);

        /// <summary>
        /// Adds a neighbour to the assimilator array if it is walkable. This method does not take diagonal rules into account, and thusly cannot be used to evaluate diagonal neighbours on its own.
        /// </summary>
        /// <param name="dx">The matrix delta x.</param>
        /// <param name="dz">The matrix delta z.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <param name="neighbours">An array to fill with neighbours.</param>
        /// <returns><c>true</c> if the neighbours was found and was added to the array; otherwise <c>false</c></returns>
        bool TryGetWalkableNeighbour(int dx, int dz, IUnitProperties unitProps, DynamicArray<IPathNode> neighbours);

        /// <summary>
        /// Gets the virtual neighbours of the node.
        /// </summary>
        /// <param name="neighbours">The virtual neighbours.</param>
        /// <param name="unitProps">The unit properties.</param>
        void GetVirtualNeighbours(DynamicArray<IPathNode> neighbours, IUnitProperties unitProps);

        /// <summary>
        /// Registers a virtual neighbour.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        void RegisterVirtualNeighbour(IPathNode neighbour);

        /// <summary>
        /// Unregisters a virtual neighbour.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        void UnregisterVirtualNeighbour(IPathNode neighbour);
    }
}
