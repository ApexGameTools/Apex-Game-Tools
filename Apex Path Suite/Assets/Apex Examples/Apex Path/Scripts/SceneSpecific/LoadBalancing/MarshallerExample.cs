/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using System.Threading;
    using Apex.LoadBalancing;
    using UnityEngine;

    /// <summary>
    /// The Marshaller allows execution of any action on the main thread from another thread.
    /// </summary>
    public class MarshallerExample : MonoBehaviour
    {
        private void DoStuff()
        {
            Debug.Log("Time is: " + Time.time);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                //Executing Unity stuff on another thread => Exception
                ThreadPool.QueueUserWorkItem((ignored) =>
                {
                    DoStuff();
                });
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                //Using the Marshaller to execute the Unity stuff on the main thread (next frame).
                ThreadPool.QueueUserWorkItem((ignored) =>
                {
                    LoadBalancer.marshaller.ExecuteOnMainThread(DoStuff);
                });
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5f, 50f, 400f, 200f), GUI.skin.window);
            GUILayout.Label("Marshaller Example\n\n");
            GUILayout.Label("Press the number key, to see the corresponding action (in the console):");
            GUILayout.Label("1. Attempting to perform a Unity action on another thread.");
            GUILayout.Label("2. Marshalling the same to the Main thread, thus succeeding.");
            GUILayout.EndArea();
        }
    }
}
