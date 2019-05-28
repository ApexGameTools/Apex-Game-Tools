/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.CostStrategy.FixedCosts
{
    using Apex.Units;
    using UnityEngine;

    public class UnitDerivedExtension : UnitComponent
    {
        [SerializeField]
        private CellCostType _affectedByCosts = CellCostType.Universal;

        public CellCostType affectedByCosts
        {
            get { return _affectedByCosts; }
            set { _affectedByCosts = value; }
        }
    }
}
