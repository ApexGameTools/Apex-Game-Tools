/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.Steering;
    using Apex.Units;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Circle Resolution", 1001)]
    public class CircleResolution : MonoBehaviour
    {
        public GameObject unitMold;
        public int unitCount = 40;

        private List<IUnitFacade> _units;

        private void Start()
        {
            _units = new List<IUnitFacade>(this.unitCount);

            const float separation = 1.5f;
            float radius = this.unitCount / (2f * Mathf.PI) * separation;
            float step = (2f * Mathf.PI) / this.unitCount;

            for (int i = 0; i < this.unitCount; i++)
            {
                float t = step * i;
                float x = Mathf.Cos(t);
                float z = Mathf.Sin(t);

                var pos = new Vector3(x, 0f, z) * radius;

                var go = Instantiate(this.unitMold, pos, Quaternion.LookRotation(pos * -1f)) as GameObject;
                go.SetActive(true);

                _units.Add(go.GetUnitFacade());
            }

            Move();
        }

        private void Move()
        {
            int limit = this.unitCount / 2;
            for (int i = 0; i < limit; i++)
            {
                var first = _units[i];
                var second = _units[i + limit];

                first.MoveTo(second.position, false);
                second.MoveTo(first.position, false);
            }
        }
    }
}
