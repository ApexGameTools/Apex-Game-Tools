/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using Apex.LoadBalancing;
    using UnityEngine;

    public class PillarToggleBalanced : MonoBehaviour, ILoadBalanced
    {
        private int _dir = 1;

        public bool repeat
        {
            // As long as this returns true the action will be executed each interval.
            // This means you can use conditionals here to control how long it runs based in e.g. number of iterations, time, state etc.
            get { return true; }
        }

        public float? ExecuteUpdate(float deltaTime, float nextInterval)
        {
            var pos = this.transform.position;
            if (pos.y > 1f)
            {
                _dir = -1;
            }
            else if (pos.y < -1f)
            {
                _dir = 1;
            }

            pos.y += 0.1f * _dir;
            this.transform.position = pos;

            //Returning null is the same as returning nextInterval, it will use whatever interval the action was scheduled with.
            //You can also conditionally return a new interval to speed execution up or slow it down.
            return null;
        }

        private void OnEnable()
        {
            _dir = 1;
            var pos = this.transform.position;
            pos.y = -1;
            this.transform.position = pos;

            LoadBalancer.defaultBalancer.Add(this);
        }

        private void OnDisable()
        {
            //An alternative to explicitly removing an item is to let Repeat return false. This will remove it after it has run one more time.
            LoadBalancer.defaultBalancer.Remove(this);
        }
    }
}
