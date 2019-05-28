/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.CostProviders
{
    using System;
    using Apex.PathFinding;
    using PathFinding.MoveCost;
    using UnityEngine;
    using WorldGeometry;

    public class SampleProviderAndFactory : MonoBehaviour, IMoveCostFactory, IMoveCost
    {
        private const int _cellMoveCost = 10;

        public int heuristicsWeight = 2;

        public int baseMoveCost
        {
            get { return _cellMoveCost; }
        }

        public int GetMoveCost(IPositioned current, IPositioned other)
        {
            var dx = Math.Abs(current.position.x - other.position.x);
            var dz = Math.Abs(current.position.z - other.position.z);
            var dy = Math.Abs(current.position.y - other.position.y);

            return Mathf.RoundToInt(this.baseMoveCost * (dx + dz + dy));
        }

        public int GetHeuristic(IPositioned current, IPositioned goal)
        {
            var dx = Math.Abs(current.position.x - goal.position.x);
            var dz = Math.Abs(current.position.z - goal.position.z);
            var dy = Math.Abs(current.position.y - goal.position.y);

            return heuristicsWeight * Mathf.RoundToInt(this.baseMoveCost * (dx + dz + dy));
        }

        public IMoveCost CreateMoveCostProvider()
        {
            return this;
        }
    }
}
