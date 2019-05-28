/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RuntimeGrids
{
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    public class BridgeSpawner : MonoBehaviour
    {
        public GameObject bridgeMold;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(90, 10, 140, 50), "Spawn Bridge"))
            {
                var go = Instantiate(this.bridgeMold, Vector3.zero, Quaternion.identity) as GameObject;
                go.SetActive(true);

                var colliders = go.GetComponentsInChildren<Collider>();

                var bridgeBounds = colliders[0].bounds;

                for (int i = 1; i < colliders.Length; i++)
                {
                    bridgeBounds.Encapsulate(colliders[i].bounds);
                }

                DebugTimer.Start();

                bridgeBounds.Update(10, true, () => DebugTimer.EndMilliseconds("Update took: {0}"));
            }
        }
    }
}
