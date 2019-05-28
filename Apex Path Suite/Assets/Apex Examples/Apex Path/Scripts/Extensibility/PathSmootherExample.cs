#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.WorldGeometry;

    public class PathSmootherExample : ISmoothPaths
    {
        /// <summary>
        /// Smooths a path.
        /// </summary>
        /// <param name="goal">The goal node of the calculated path.</param>
        /// <param name="maxPathLength">Maximum length of the path.</param>
        /// <param name="request">The path request.</param>
        /// <param name="costStrategy">The cell cost provider.</param>
        /// <returns>
        /// The path in smoothed form
        /// </returns>
        public Path Smooth(IPathNode goal, int maxPathLength, IPathRequest request, ICellCostStrategy costStrategy)
        {
            //The goal node represents the end of the path. Using it you can traverse backwards using the predecessor property,
            //until you reach the start node, which is identified by having no predecessor.
            //Your aim then is to process the nodes and remove superfluous nodes and add the significant nodes to the resulting stack.

            //For this example we simply do nothing at all and return the path as it was intended by the path finder
            var result = new Path(maxPathLength);
            var node = goal;

            while (node != null)
            {
                result.Push(node);
                node = node.predecessor;
            }

            return result;
        }
    }
}
