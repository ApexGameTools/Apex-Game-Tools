/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.CostStrategy.FixedCosts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum CellCostType
    {
        Universal = 0,

        RedTeamInfluenceZone = 1,
        BlueTeamInfluenceZone = 2,

        LightDamage = 16,
        MediumDamage = 32,
        HighDamage = 64,
    }
}
