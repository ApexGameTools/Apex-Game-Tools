/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.LoadBalancing
{
    using UnityEngine;

    public class LoadBalancerBasicDemoControl : MonoBehaviour
    {
        public GameObject ExamplesHost;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5f, 0f, Screen.width - 5f, 40f));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Update", GUILayout.Height(30f)))
            {
                Toggle<PillarToggle>();
            }

            GUILayout.Space(6f);

            if (GUILayout.Button("Coroutine", GUILayout.Height(30f)))
            {
                Toggle<PillarToggleCoroutine>();
            }

            GUILayout.Space(6f);

            if (GUILayout.Button("Load Balancer", GUILayout.Height(30f)))
            {
                Toggle<PillarToggleBalanced>();
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

            var all = ExamplesHost.GetComponentsInChildren<MonoBehaviour>();
            foreach (var b in all)
            {
                b.enabled = (b is T);
            }
        }
    }
}
