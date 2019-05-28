/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/Navigation/Vector Field Navigation", 208)]
    public sealed class VectorFieldNavigationQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            QuickStarts.VectorFieldNavigation(this.gameObject, !isPrefab);
            return null;
        }
    }
}
