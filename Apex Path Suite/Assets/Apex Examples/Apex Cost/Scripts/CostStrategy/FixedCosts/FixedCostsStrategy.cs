/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.CostStrategy.FixedCosts
{
    using Apex.PathFinding;
    using Apex.WorldGeometry;

    public class FixedCostsStrategy : ICellCostStrategy
    {
        public int GetCellCost(IGridCell cell, object unitProperties)
        {
            //First cast the properties to the type we expect to see. The commented line is for using the alternative Unit type from UnitPartialExtension.cs.
            var concreteProperties = unitProperties as UnitDerivedExtension;
            /* var concreteProperties = unitProperties as UnitComponent; */

            var mask = cell.cost & (int)concreteProperties.affectedByCosts;
            return CellCostManager.instance.GetCost(mask);
        }
    }
}
