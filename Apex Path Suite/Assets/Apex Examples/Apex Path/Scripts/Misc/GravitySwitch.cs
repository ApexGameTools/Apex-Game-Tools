#pragma warning disable 1591
namespace Apex.Examples.Misc
{
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("")]
    public class GravitySwitch : MonoBehaviour
    {
        public float gravityHeight;

        private Dictionary<Collider, UnitEntry> _unitsInTrigger = new Dictionary<Collider, UnitEntry>();

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.GetComponent<Rigidbody>();
            if (rb == null)
            {
                return;
            }

            var entry = new UnitEntry
            {
                unit = rb,
                gravityEnabled = rb.useGravity
            };

            _unitsInTrigger[other] = entry;
            rb.useGravity = (other.transform.position.y >= this.gravityHeight);
        }

        private void OnTriggerExit(Collider other)
        {
            UnitEntry entry;
            if (_unitsInTrigger.TryGetValue(other, out entry))
            {
                _unitsInTrigger.Remove(other);
                entry.unit.useGravity = entry.gravityEnabled;
            }
        }

        private class UnitEntry
        {
            public Rigidbody unit;
            public bool gravityEnabled;
        }
    }
}
