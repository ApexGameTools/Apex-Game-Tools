/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using System.Collections;
    using Apex.LoadBalancing;
    using UnityEngine;

    /// <summary>
    /// Long running actions can be thought of as load balanced coroutines which are then themselves load balanced.
    /// </summary>
    public class LongRunningActionExample : MonoBehaviour
    {
        private ILoadBalancedHandle _handle;

        private IEnumerator DoWork()
        {
            //Doing 100k raycasts would kill the frame rate under normal circumstances, but doing it like this allows full control.
            for (int i = 0; i < 100000; i++)
            {
                Physics.Raycast(Vector3.zero, Vector3.down);
                yield return null;
            }

            Debug.Log("Done.");
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                //So here we create a long running action which will complete the DoWork call but only use 3 ms per frame, hence spreading it out across numerous frames.
                var action = new LongRunningAction(DoWork, 3);

                //We schedule it to run each frame if possible
                _handle = ExamplesLoadBalancer.extraBalancer.Add(action, 0f);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                //Here we stop the action initiated in 3 above
                if (_handle != null)
                {
                    _handle.Stop();
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5f, 50f, 400f, 200f), GUI.skin.window);
            GUILayout.Label("Long Running Action Example\n\n");
            GUILayout.Label("Press the number key, to see the corresponding action (in the console):");
            GUILayout.Label("1. Long running, 3ms per frame.");
            GUILayout.Label("4. Stop long running.");
            GUILayout.EndArea();
        }
    }
}
