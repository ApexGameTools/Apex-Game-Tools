#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.PathFinding;
    using Apex.PathFinding.MoveCost;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Extensibility/Move Cost Factory Example", 1005)]
    public class MoveCostFactoryExample : MonoBehaviour, IMoveCostFactory
    {
        public IMoveCost CreateMoveCostProvider()
        {
            //The the path engine to use this cost provider instead of the default (DiagonalDistance)
            //If you create you own cost provider, you simply return an instance of that instead.
            return new EuclideanDistance(10);
        }
    }
}
