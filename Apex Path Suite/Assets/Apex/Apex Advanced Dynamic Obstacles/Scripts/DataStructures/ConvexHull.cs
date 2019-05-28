/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Represents a Convex Hull of points on the xz plane, i.e. the outline of a number of points projected onto the xz plane.
    /// </summary>
    public class ConvexHull
    {
        private readonly Vector3[] _points;
        private readonly PolarAngleComparer _comparer;
        private readonly bool _isVelocityGrowthEnabled;
        private int _hullCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvexHull"/> class.
        /// </summary>
        /// <param name="numberOfPoints">The number of points from which to create the hull.</param>
        /// <param name="enableVelocityGrowth">Controls if the hull will accept to be grown by a velocity via <see cref="CalculateHull(UnityEngine.Vector3)"/></param>
        public ConvexHull(int numberOfPoints, bool enableVelocityGrowth)
        {
            _isVelocityGrowthEnabled = enableVelocityGrowth;
            if (enableVelocityGrowth)
            {
                numberOfPoints *= 2;
            }

            _points = new Vector3[numberOfPoints + 1];
            _comparer = new PolarAngleComparer();
        }

        /// <summary>
        /// Gets the number of points in the hull.
        /// </summary>
        /// <value>
        /// The hull points count.
        /// </value>
        public int hullPointsCount
        {
            get { return _hullCount; }
            set { _hullCount = value; }
        }

        /// <summary>
        /// Gets the point with the specified index. There is no bounds checking.
        /// </summary>
        /// <param name="idx">The index.</param>
        /// <returns>The point at the index</returns>
        public Vector3 this[int idx]
        {
            get { return _points[idx]; }
            set { _points[idx] = value; }
        }

        /// <summary>
        /// Mirrors the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Mirror(ConvexHull source)
        {
            _hullCount = source._hullCount;

            for (int i = 0; i < _hullCount; i++)
            {
                _points[i] = source[i];
            }
        }

        /// <summary>
        /// Resets the hull.
        /// </summary>
        public void Reset()
        {
            _hullCount = 0;
        }

        /// <summary>
        /// Calculates the hull, grown by a vector. If not created with enableVelocityGrowth = true, this will throw an exception.
        /// </summary>
        public void CalculateHull(Vector3 growBy)
        {
            int growthOffset = (_points.Length - 1) / 2;
            for (int i = 0; i < growthOffset; i++)
            {
                _points[i + growthOffset] = _points[i] + growBy;
            }

            CalculateHull(_points.Length - 1);
        }

        /// <summary>
        /// Calculates the hull.
        /// </summary>
        public void CalculateHull()
        {
            var pointCount = (_points.Length - 1);
            if (_isVelocityGrowthEnabled)
            {
                pointCount /= 2;
            }

            CalculateHull(pointCount);
        }

        /// <summary>
        /// Determines whether the hull contains the specified point.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        public bool Contains(Vector3 test)
        {
            int i;
            int j;
            bool result = false;
            for (i = 0, j = _hullCount - 1; i < _hullCount; j = i++)
            {
                if ((_points[i].z > test.z) != (_points[j].z > test.z) &&
                    (test.x < ((_points[j].x - _points[i].x) * (test.z - _points[i].z) / (_points[j].z - _points[i].z)) + _points[i].x))
                {
                    result = !result;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the bounds of the hull.
        /// </summary>
        /// <returns></returns>
        public Bounds CalculateBounds()
        {
            Vector3 pmax = _points[0];
            Vector3 pmin = pmax;

            for (int i = 1; i < _hullCount; i++)
            {
                var p = _points[i];
                if (p.x > pmax.x)
                {
                    pmax.x = p.x;
                }
                else if (p.x < pmin.x)
                {
                    pmin.x = p.x;
                }

                if (p.z > pmax.z)
                {
                    pmax.z = p.z;
                }
                else if (p.z < pmin.z)
                {
                    pmin.z = p.z;
                }

                if (p.y > pmax.y)
                {
                    pmax.y = p.y;
                }
                else if (p.y < pmin.y)
                {
                    pmin.y = p.y;
                }
            }

            var size = pmax - pmin + new Vector3(.1f, .1f, .1f);
            return new Bounds(pmin + (size / 2f), size);
        }

        private static float TurnDir(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return ((p2.x - p1.x) * (p3.z - p1.z)) - ((p2.z - p1.z) * (p3.x - p1.x));
        }

        private void CalculateHull(int count)
        {
            //Find the bottom leftmost point, i.e. point with smallest z and x in that order
            var p = _points[0];
            for (int i = 1; i < count; i++)
            {
                var other = _points[i];
                if (other.z < p.z || (other.z == p.z && other.x < p.x))
                {
                    _points[0] = other;
                    _points[i] = p;
                    p = other;
                }
            }

            //Sort the points by polar angle in relation to the start point
            _comparer.Prepare(p);
            Array.Sort(_points, 1, count - 1, _comparer);

            //Set the last extra element to that of the first to account for collinearity on the z-axis
            _points[count] = _points[0];
            _hullCount = 1;

            //Build the hull
            for (int i = 2; i <= count; i++)
            {
                while (TurnDir(_points[_hullCount - 1], _points[_hullCount], _points[i]) <= 0f)
                {
                    if (_hullCount > 1)
                    {
                        _hullCount--;
                    }
                    else if (i == count)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }

                _hullCount++;

                if (i != _hullCount)
                {
                    //Since we are only interested in the actual hull, there is no need to swap here, just assign
                    _points[_hullCount] = _points[i];
                }
            }
        }

        private sealed class PolarAngleComparer : IComparer<Vector3>
        {
            private Vector3 _p;

            internal void Prepare(Vector3 refPoint)
            {
                _p = refPoint;
            }

            public int Compare(Vector3 p1, Vector3 p2)
            {
                var v1 = p1 - _p;
                var v2 = p2 - _p;
                var pa1 = PolarAngleX(v1);
                var pa2 = PolarAngleX(v2);

                if (pa1 > pa2)
                {
                    return 1;
                }

                if (pa1 < pa2)
                {
                    return -1;
                }

                var v1mag = v1.sqrMagnitude;
                var v2mag = v2.sqrMagnitude;

                if (v1mag < v2mag)
                {
                    return 1;
                }

                if (v1mag > v2mag)
                {
                    return -1;
                }

                return 0;
            }

            private static double PolarAngleX(Vector3 v)
            {
                if (v.x > 0f)
                {
                    return Math.Atan(v.z / v.x);
                }

                if (v.x < 0f && v.z >= 0f)
                {
                    return Math.Atan(v.z / v.x) + Math.PI;
                }

                //Note that contrary to 'normal' we treat x=0 and z=0 the same as other points on the z axis at x=0
                //This means that if multiple points exist at the start point, the excess points are evaluated as having a polar distance of 90 deg instead of 0.
                if (v.x == 0f && v.z > 0f)
                {
                    return Math.PI / 2f;
                }

                //Not accurate but since we only need to handle cases of positive or 0 z values, the remaining atan2 is left out.
                //We instead use this value to ensure that correlated starting points do not occur
                //so if any point is identical to the reference they are put last (high value above being PI, a value of for ensures this).
                return 4f;
            }
        }
    }
}
