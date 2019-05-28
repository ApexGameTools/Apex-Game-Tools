/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using UnityEngine;

    public class LoadBalancerAdvancedDemoControl : MonoBehaviour
    {
        public GameObject ExamplesHost;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5f, 0f, Screen.width - 5f, 40f));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("One Time Action", GUILayout.Height(30f)))
            {
                Toggle<OneTimeActionExample>();
            }

            GUILayout.Space(6f);

            if (GUILayout.Button("Repeatable Action", GUILayout.Height(30f)))
            {
                Toggle<RepetableActionExample>();
            }

            GUILayout.Space(6f);

            if (GUILayout.Button("Long Running Action", GUILayout.Height(30f)))
            {
                Toggle<LongRunningActionExample>();
            }

            GUILayout.Space(6f);

            if (GUILayout.Button("Pooled Actions", GUILayout.Height(30f)))
            {
                Toggle<PooledActionsExample>();
            }

            GUILayout.Space(6f);

            if (GUILayout.Button("Marshaller", GUILayout.Height(30f)))
            {
                Toggle<MarshallerExample>(); 
            }

            GUILayout.Space(6f);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void Toggle<T>() where T : MonoBehaviour
        {
            if (ExamplesHost == null)
            {
                Debug.LogWarning("Examples Host not set on demo controller.");
                return;
            }

            var all = ExamplesHost.GetComponents<MonoBehaviour>();
            foreach (var b in all)
            {
                b.enabled = (b is T);
            }
        }
    }
}
