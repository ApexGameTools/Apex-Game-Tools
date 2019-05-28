/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using System.Collections.Generic;
    using Apex.WorldGeometry;
    using UnityEngine;

    internal class DistanceComparer<T> : IComparer<T> where T : IPositioned
    {
        private int _sortDir = 1;

        public Vector3 compareTo;

        public DistanceComparer(bool ascending)
        {
            if (!ascending)
            {
                _sortDir = -1;
            }
        }

        public int Compare(T a, T b)
        {
            if (a == null)
            {
                return 1 * _sortDir;
            }

            if (b == null)
            {
                return -1 * _sortDir;
            }

            return (a.position - this.compareTo).sqrMagnitude
                .CompareTo((b.position - this.compareTo).sqrMagnitude) * _sortDir;
        }
    }
}