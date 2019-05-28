/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.Follow
{
    using Apex;
    using Apex.LoadBalancing;
    using UnityEngine;

    public class FollowCommands : MonoBehaviour
    {
        public Transform target;

        private ILoadBalancedHandle _followHandle;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                var unit = this.GetUnitFacade();
                _followHandle = unit.Follow(target);
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                var unit = this.GetUnitFacade();
                unit.Stop();
                _followHandle.Stop();
            }
        }
    }
}
