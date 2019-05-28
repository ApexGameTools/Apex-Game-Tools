/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/Navigation/Navigating Unit on Patrol", 202)]
    public sealed class NavigatingUnitOnPatrolQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            QuickStarts.NavigatingUnitOnPatrol(this.gameObject, !isPrefab);
            return null;
        }
    }
}
