/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using System.Collections;
    using Apex.LoadBalancing;
    using UnityEngine;

    public class PillarToggleCoroutine : MonoBehaviour
    {
        public float secondDelay = 0.1f;

        private int _dir = 1;
        private Coroutine _routine;
        private WaitForSeconds _delay;

        public IEnumerator Execute()
        {
            while (true)
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

                yield return _delay;
            }
        }

        private void OnEnable()
        {
            _dir = 1;
            var pos = this.transform.position;
            pos.y = -1;
            this.transform.position = pos;

            _delay = new WaitForSeconds(secondDelay);
            _routine = StartCoroutine(Execute());
        }

        private void OnDisable()
        {
            StopCoroutine(_routine);
        }
    }
}
