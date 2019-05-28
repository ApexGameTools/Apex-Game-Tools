/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.CostStrategy.FixedCosts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class CellCostManager
    {
        public static readonly CellCostManager instance = new CellCostManager();

        private readonly int _costCount;
        private readonly int[] _costs;

        private CellCostManager()
        {
            //Size the array to the required size. No need to have entries above the highest possible bit
            var vals = Enum.GetValues(typeof(CellCostType));
            _costCount = (int)Math.Log((int)vals.GetValue(vals.Length - 1), 2) + 2;
            _costs = new int[_costCount];

            //This of course could be made more dynamic, but it suffices for this example
            SetCost(CellCostType.BlueTeamInfluenceZone, 500);
            SetCost(CellCostType.RedTeamInfluenceZone, 500);

            SetCost(CellCostType.LightDamage, 100);
            SetCost(CellCostType.MediumDamage, 500);
            SetCost(CellCostType.HighDamage, 1000);
        }

        public int GetCost(int costMask)
        {
            //Always apply the universal cost
            int sum = _costs[0];
            if (costMask > 0)
            {
                for (int i = 1; i < _costCount; i++)
                {
                    if (((1 << (i - 1)) & costMask) > 0)
                    {
                        sum += _costs[i];
                    }
                }
            }

            return sum;
        }

        private void SetCost(CellCostType t, int cost)
        {
            var idx = (int)Math.Log((int)t, 2) + 1;
            _costs[idx] = cost;
        }
    }
}
