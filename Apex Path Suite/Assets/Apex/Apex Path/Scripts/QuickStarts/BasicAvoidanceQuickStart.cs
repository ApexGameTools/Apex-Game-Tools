/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/Navigation/Basic Avoidance", 300)]
    public sealed class BasicAvoidanceQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            QuickStarts.BasicAvoidance(this.gameObject, !isPrefab);
            return null;
        }
    }
}
