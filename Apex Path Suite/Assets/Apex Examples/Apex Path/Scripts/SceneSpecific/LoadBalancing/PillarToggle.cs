/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using UnityEngine;

    public class PillarToggle : MonoBehaviour
    {
        private int _dir = 1;

        private void OnEnable()
        {
            _dir = 1;
            var pos = this.transform.position;
            pos.y = -1;
            this.transform.position = pos;
        }

        private void Update()
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
        }
    }
}
