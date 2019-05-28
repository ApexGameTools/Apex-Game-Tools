/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using UnityEngine;

    /// <summary>
    /// Represents an item that can be picked up
    /// </summary>
    [AddComponentMenu("Apex/Examples/Pickup Item", 1016)]
    public class PickupItem : MonoBehaviour
    {
        private Collider _collider;

        private void Start()
        {
            var master = FindObjectOfType<PickupMaster>();
            if (master != null)
            {
                master.RegisterItem(this);
            }

            _collider = this.GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var worker = other.GetComponent<PickupWorker>();
            if (worker != null)
            {
                if (worker.Pickup(this))
                {
                    _collider.enabled = false;
                }
            }
        }
    }
}
