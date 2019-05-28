/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/Navigation/Navigating Unit Wandering", 203)]
    public sealed class NavigatingUnitWanderingQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            QuickStarts.NavigatingUnitWandering(this.gameObject, !isPrefab);
            return null;
        }
    }
}
