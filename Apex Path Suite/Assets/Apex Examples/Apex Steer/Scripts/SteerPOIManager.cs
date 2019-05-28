/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/POI Manager", 1019)]
    public class SteerPOIManager : MonoBehaviour
    {
        private List<Vector3> _pois;

        private void Awake()
        {
            _pois = new List<Vector3>();

            foreach (var fc in GetComponentsInChildren<SteerFormationChanger>())
            {
                _pois.Add(fc.transform.position);
            }
        }

        public Vector3 GetPOI(Vector3 skip)
        {
            var poi = skip;

            do
            {
                var idx = Random.Range(0, _pois.Count);
                poi = _pois[idx];
            }
            while (poi == skip);

            return poi;
        }
    }
}
