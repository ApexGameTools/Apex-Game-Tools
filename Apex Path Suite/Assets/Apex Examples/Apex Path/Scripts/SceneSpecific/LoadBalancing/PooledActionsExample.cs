/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using System.Collections;
    using System.Collections.Generic;
    using Apex.LoadBalancing;
    using UnityEngine;

    /// <summary>
    /// The pooled extension of the load balancer can be used to schedule actions for execution in an easy and allocation free way.
    /// </summary>
    public class PooledActionsExample : MonoBehaviour
    {
        private Stack<ILoadBalancedHandle> _runningActions = new Stack<ILoadBalancedHandle>();

        private void DoWork()
        {
            Debug.Log("Work Done.");
        }

        private bool DoContinuousWork(float deltaTime)
        {
            Debug.Log("Work Done.");

            //We do this forever
            return true;
        }

        private IEnumerator WorkingLong()
        {
            Debug.Log("Starting long work cycle.");

            //Busy wait for 20 seconds, obviously you would do actual work here.
            var stopTime = Time.time + 20f;
            while (Time.time < stopTime)
            {
                yield return null;
            }

            Debug.Log("Ended long work cycle.");
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                //Execute an action once using a pooled wrapper (i.e. consecutive executions of actions does not allocate)
                ExamplesLoadBalancer.examplesBalancer.ExecuteOnce(DoWork);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                //Execute an action once after a three second delay
                ExamplesLoadBalancer.examplesBalancer.ExecuteOnce(() => Debug.Log("Work Done via Lambda."), 3f);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                //Execute this using the default interval of the used load balancer with no starting delay
                var handle = LoadBalancer.defaultBalancer.Execute(DoContinuousWork);

                _runningActions.Push(handle);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                //Execute the anonymous action for 10 seconds, using a custom interval of 3 seconds and an identical start delay
                var stopTime = Time.time + 10f;
                var handle = LoadBalancer.defaultBalancer.Execute(
                    (dt) =>
                    {
                        Debug.Log("Working hard after waiting for " + dt + " seconds");
                        return Time.time < stopTime;
                    },
                    3f,
                    true);

                _runningActions.Push(handle);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha5))
            {
                //Execute a long running action, allowing it to use 4ms per frame.
                //Note that the long running action is disconnected from the load balancer settings. This means that if the load balancer has a budget of 5 ms per frame,
                //uses 3 of them on other actions and then starts this long running action, it will end up using 7 ms that frame.
                var handle = LoadBalancer.defaultBalancer.Execute(WorkingLong(), 4);

                _runningActions.Push(handle);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha6))
            {
                //We start by cleaning out any handles that have been disposed, i.e. the action has completed.
                while (_runningActions.Count > 0 && _runningActions.Peek().isDisposed)
                {
                    _runningActions.Pop();
                }

                //Next we stop the next running action. Actions are stopped in a FILO manner.
                if (_runningActions.Count > 0)
                {
                    _runningActions.Pop().Stop();
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5f, 50f, 400f, 300f), GUI.skin.window);
            GUILayout.Label("Pooled Actions Example\n\n");
            GUILayout.Label("Press the number key, to see the corresponding action (in the console):");
            GUILayout.Label("1. Execute an action once (next frame)");
            GUILayout.Label("2. Delayed execution (3 seconds)");
            GUILayout.Label("3. Continuous work (repeats forever)");
            GUILayout.Label("4. Repeats work for 10 seconds at a 3 second interval.");
            GUILayout.Label("5. Runs a long running action which takes 20 seconds to complete, using only 4 ms per frame.");
            GUILayout.Label("6. Stop one repeated/long running action.");
            GUILayout.EndArea();
        }
    }
}
