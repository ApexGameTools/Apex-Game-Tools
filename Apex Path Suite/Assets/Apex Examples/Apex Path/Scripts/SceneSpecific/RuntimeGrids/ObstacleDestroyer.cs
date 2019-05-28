/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RuntimeGrids
{
    using Apex.WorldGeometry;
    using UnityEngine;

    public class ObstacleDestroyer : MonoBehaviour
    {
        private void OnCollisionEnter(Collision col)
        {
            var other = col.collider;

            if (!Layers.InLayer(other.gameObject, Layers.blocks))
            {
                return;
            }

            GridManager.instance.Update(other.bounds, 10);

            Destroy(other.gameObject);
            Destroy(this.gameObject);
        }
    }
}
