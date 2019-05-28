/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using Apex.LoadBalancing;
    using UnityEngine;

    /// <summary>
    /// The RepetableAction can be used to schedule an action that repeats a number of times (or indefinitely).
    /// </summary>
    public class RepetableActionExample : MonoBehaviour
    {
        private int _counter;
        private ILoadBalancedHandle _handle;

        private bool DoWork(float deltaTime)
        {
            Debug.Log("Execution " + ++_counter);

            //The return value determines if the action will continue to repeat.
            return true;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                _counter = 0;

                //Here we schedule some work to repeat 4 times, i.e. it will execute a total of 5 times.
                var action = new RepeatableAction(DoWork, 4);

                //Here we schedule the action to start as soon as possible and repeat at the default interval of the load balancer.
                ExamplesLoadBalancer.examplesBalancer.Add(action);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                //Here we create an action using an anonymous method. We also do not specify a repetition count, instead we control the life time in the method.
                int counter = 0;
                var action = new RepeatableAction(
                    (t) =>
                    {
                        Debug.Log("Execution " + ++counter + " done after " + t.ToString() + " seconds");
                        return counter < 5;
                    });

                //Here we schedule the action to take place after a short delay of 3 seconds or as soon after that delay as possible.
                //It will then repeat with an interval of 2 seconds.
                ExamplesLoadBalancer.examplesBalancer.Add(action, 2f, 3f);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                //Here we create an action using an anonymous method. We also do not specify a repetition count, and instruct it to continue indefinitely.
                //The only way to stop it is to use the handle returned from the load balancer when the action is added.
                int counter = 0;
                var action = new RepeatableAction(
                    (t) =>
                    {
                        Debug.Log("Execution " + ++counter + " done after " + t.ToString() + " seconds");
                        return true;
                    });

                //Here we schedule the action to take as soon as possible.
                //It will then repeat with an interval of 2 seconds.
                _handle = ExamplesLoadBalancer.examplesBalancer.Add(action, 2f);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha4))
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
            GUILayout.BeginArea(new Rect(5f, 50f, 400f, 250f), GUI.skin.window);
            GUILayout.Label("Repeatable Action Example\n\n");
            GUILayout.Label("Press the number key, to see the corresponding action (in the console):");
            GUILayout.Label("1. Fixed repetitions (5 times), default interval.");
            GUILayout.Label("2. Conditional repetitions, 2 sec interval, 3 sec initial delay.");
            GUILayout.Label("3. Indefinite, 2 sec interval.");
            GUILayout.Label("4. Stop indefinite.");
            GUILayout.EndArea();
        }
    }
}
