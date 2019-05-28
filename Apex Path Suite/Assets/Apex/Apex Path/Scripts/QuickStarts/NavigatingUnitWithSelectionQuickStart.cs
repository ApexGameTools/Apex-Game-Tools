/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/Navigation/Navigating Unit with Selection", 201)]
    public sealed class NavigatingUnitWithSelectionQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            QuickStarts.NavigatingUnitWithSelection(this.gameObject, !isPrefab);
            return null;
        }
    }
}
