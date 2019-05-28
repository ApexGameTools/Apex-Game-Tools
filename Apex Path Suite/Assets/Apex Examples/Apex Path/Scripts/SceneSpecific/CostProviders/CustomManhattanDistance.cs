#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.CostProviders
{
    using Apex.WorldGeometry;
    using PathFinding.MoveCost;

    /// <summary>
    /// Manhattan distance heuristic, i.e. only horizontal / vertical movement. Assumes distance between cells along axis are whole numbers, i.e. a uniform grid.
    /// Cell move cost constant D (_cellMoveCost) is used.
    /// </summary>
    public class CustomManhattanDistance : ManhattanDistance
    {
        private int _heuristicsWeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomManhattanDistance"/> class.
        /// </summary>
        /// <param name="cellMoveCost">The cost to move from one cell to an adjacent cell parallel to ONE axis, i.e. not diagonally</param>
        public CustomManhattanDistance(int cellMoveCost, int heuristicsWeight)
            : base(cellMoveCost)
        {
            _heuristicsWeight = heuristicsWeight;
        }

        /// <summary>
        /// Gets the heuristic.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="goal">The goal.</param>
        /// <returns>The heuristic</returns>
        public override int GetHeuristic(IPositioned current, IPositioned goal)
        {
            return _heuristicsWeight * base.GetHeuristic(current, goal);
        }
    }
}
