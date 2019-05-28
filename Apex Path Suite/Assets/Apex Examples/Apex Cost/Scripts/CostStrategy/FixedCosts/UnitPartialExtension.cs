/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Units
{
    using Apex.Examples.CostStrategy.FixedCosts;
    using UnityEngine;

    // One option when defining a custom unit component, is to simply expand the UnitComponent class itself.
    // This is possible since it is defined as partial. The pro of doing this, is that it will mean that all units will have these additional properties, methods etc.
    // without having to replace the UnitComponent with another type.
    // The downside is that it may not be clear to all what is happening.
    // The implementation is commented out, since when doing this all units in all scenes will be affected.
    // To try it out, simply uncomment the rest from here:
    //public partial class UnitComponent
    //{
    //    [SerializeField]
    //    private CellCostType _affectedByCosts = CellCostType.Universal;

    //    public CellCostType affectedByCosts
    //    {
    //        get { return _affectedByCosts; }
    //        set { _affectedByCosts = value; }
    //    }
    //}
}
