/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Components
{
    using UnityEngine;

    [AddComponentMenu("Apex/Quick Starts/AI/Utility AI Client", 100)]
    public class UtilityAIClientQuickStart : ApexQuickStartComponent
    {
        public override GameObject Apply(bool isPrefab)
        {
            AIQuickStarts.UtilityAIClient(this.gameObject, !isPrefab);
            return null;
        }
    }
}
