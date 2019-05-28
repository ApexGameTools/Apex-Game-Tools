/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using Apex.LoadBalancing;
    using UnityEngine;

    /// <summary>
    /// The OneTimeAction can be used to schedule an action that runs a single time.
    /// </summary>
    public class OneTimeActionExample : MonoBehaviour
    {
        private void DoWork(float deltaTime)
        {
            Debug.Log("Work Done.");
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                //You can pass it a matching named method
                var action = new OneTimeAction(DoWork);

                //Here we schedule the action to take place as soon as possible. Since this is a one time action, the interval is irrelevant.
                ExamplesLoadBalancer.examplesBalancer.Add(action);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                //You can also pass it an anonymous method.
                var action = new OneTimeAction(
                    (t) => Debug.Log("Work done after " + t.ToString() + " seconds"));

                //Here we schedule the action to take place after a short delay or as soon after that delay as possible.
                //Since this is a one time action, the interval is irrelevant but in this case used as the delay.
                ExamplesLoadBalancer.examplesBalancer.Add(action, 2f, true);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5f, 50f, 400f, 200f), GUI.skin.window);
            GUILayout.Label("One Time Action Example\n\n");
            GUILayout.Label("Press the number key, to see the corresponding action (in the console):");
            GUILayout.Label("1. Immediate Execution (next frame)");
            GUILayout.Label("2. Delayed execution (2 seconds)");
            GUILayout.EndArea();
        }
    }
}
