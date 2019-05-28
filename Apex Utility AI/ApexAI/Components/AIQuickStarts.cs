/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Components
{
    using LoadBalancing;
    using UnityEngine;
    using Utilities;

    internal class AIQuickStarts
    {
        internal static void LoadBalancer(GameObject target)
        {
            var lb = ComponentHelper.FindFirstComponentInScene<LoadBalancerComponent>();
            if (lb != null)
            {
                return;
            }
            else if (target != null)
            {
                target.AddComponent<LoadBalancerComponent>();
            }
            else
            {
                var go = new GameObject("Load Balancer");
                go.AddComponent<LoadBalancerComponent>();
                Debug.Log("No Load Balancer found, creating one.");
            }
        }

        internal static void UtilityAIClient(GameObject target, bool ensureLoadBalancer)
        {
            target.AddIfMissing<UtilityAIComponent>();

            if (ensureLoadBalancer)
            {
                LoadBalancer(null);
            }
        }
    }
}
