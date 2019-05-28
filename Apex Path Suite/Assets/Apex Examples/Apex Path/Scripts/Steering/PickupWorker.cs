/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Represents a worker that when directed by its master, will pick up items.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Pickup Worker", 1018)]
    public class PickupWorker : MonoBehaviour
    {
        private IUnitFacade _unit;
        private PickupMaster _master;
        private PickupItem _target;
        private PickupItem _payload;

        public IUnitFacade unit
        {
            get { return _unit; }
        }

        private void Awake()
        {
            _master = FindObjectOfType<PickupMaster>();
        }

        private void Start()
        {
            _unit = this.GetUnitFacade();

            if (_master != null)
            {
                _master.RegisterWorker(this);
            }
        }

        public void SetTarget(PickupItem item)
        {
            _target = item;
        }

        public bool Pickup(PickupItem item)
        {
            if (_target != item)
            {
                return false;
            }

            _target = null;
            _payload = item;

            _payload.transform.parent = this.transform;
            _payload.transform.localPosition = Vector3.up;

            _master.PickupComplete(this);

            return true;
        }

        public PickupItem OffLoad()
        {
            if (_payload == null)
            {
                return null;
            }

            _payload.transform.parent = null;
            var item = _payload;
            _payload = null;

            return item;
        }
    }
}
