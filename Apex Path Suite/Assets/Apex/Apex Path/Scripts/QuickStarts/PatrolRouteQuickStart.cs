/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/Navigation/Patrol Route", 102)]
    public sealed class PatrolRouteQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            return QuickStarts.PatrolRoute(this.gameObject);
        }
    }
}
