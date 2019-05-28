/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using System.Collections.Generic;
    using Apex.Steering;
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;
    using WorldGeometry;
    [AddComponentMenu("Apex/Examples/Formation Changer", 1006)]
    public class SteerFormationChanger : MonoBehaviour
    {
        private SteerPOIManager _poiSource;
        private Dictionary<object, float> _recentlyChanged = new Dictionary<object, float>();

        public float formationRadius = 2f;
        public int waypointIterations = 25;

        private void Awake()
        {
            _poiSource = GetComponentInParent<SteerPOIManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Layers.InLayer(other.gameObject, Layers.units))
            {
                return;
            }

            var unit = other.GetUnitFacade();
            if (unit == null)
            {
                return;
            }

            var grp = unit.transientGroup;
            if (grp == null)
            {
                return;
            }

            if (grp.currentWaypoints.count <= 1)
            {
                Vector3 lastPOI = _poiSource.GetPOI(this.transform.position);
                for (int i = 0; i < waypointIterations; i++)
                {
                    grp.MoveTo(lastPOI, true);
                    lastPOI = _poiSource.GetPOI(lastPOI);
                }
            }

            float lastChanged;
            if (_recentlyChanged.TryGetValue(grp, out lastChanged) && (Time.time - lastChanged) < 10f)
            {
                return;
            }

            _recentlyChanged[grp] = Time.time;

            var randomFormation = grp.currentFormation == null ? GetFormation(Random.Range(0, 6)) : GetRandomFormation(grp.currentFormation);
            grp.SetFormation(randomFormation);
        }

        private IFormation GetRandomFormation(IFormation skip)
        {
            var form = skip;

            do
            {
                var idx = Random.Range(0, 6);
                form = GetFormation(idx);
            }
            while (form.GetType() == skip.GetType());

            return form;
        }

        private IFormation GetFormation(int idx)
        {
            switch (idx)
            {
                case 0:
                {
                    return new FormationEllipsoid(this.formationRadius);
                }

                case 1:
                {
                    return new FormationGrid(this.formationRadius);
                }

                case 2:
                {
                    return new FormationRow(this.formationRadius);
                }

                case 3:
                {
                    return new FormationLine(this.formationRadius);
                }

                case 4:
                {
                    return new FormationSpiral(this.formationRadius);
                }

                default:
                {
                    return new FormationWing(this.formationRadius);
                }
            }
        }
    }
}