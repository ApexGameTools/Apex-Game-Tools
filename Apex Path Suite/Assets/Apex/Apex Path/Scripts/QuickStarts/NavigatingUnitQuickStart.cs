/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/Navigation/Navigating Unit", 200)]
    public sealed class NavigatingUnitQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            QuickStarts.NavigatingUnit(this.gameObject, !isPrefab);
            return null;
        }
    }
}
