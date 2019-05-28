/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RuntimeGrids
{
    using Apex.WorldGeometry;
    using UnityEngine;

    public class BombSpawner : MonoBehaviour
    {
        public GameObject bombMold;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(90, 10, 140, 50), "Spawn Bomb"))
            {
                var obstacles = Physics.OverlapSphere(Vector3.zero, 40f, Layers.blocks);
                if (obstacles.Length == 0)
                {
                    return;
                }

                var target = obstacles[0];

                var go = Instantiate(this.bombMold, Vector3.zero, Quaternion.identity) as GameObject;
                go.SetActive(true);

                var rb = go.GetComponent<Rigidbody>();
                rb.AddForce(target.transform.position.OnlyXZ() * 50f, ForceMode.Force);
            }
        }
    }
}
