#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.CostProviders
{
    using Apex.PathFinding;
    using PathFinding.MoveCost;
    using UnityEngine;

    public class SampleMoveCostProviderFactory : MonoBehaviour, IMoveCostFactory
    {
        public int heuristicsWeight = 2;

        public IMoveCost CreateMoveCostProvider()
        {
            return new CustomManhattanDistance(10, heuristicsWeight);
        }
    }
}
