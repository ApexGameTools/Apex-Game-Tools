/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using System;
    using Apex.Common;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Height provider for box colliders that samples five points, corners and center of the collider.
    /// </summary>
    public class RaycastBoxFivePointProvider : IUnitHeightProvider
    {
        private Vector3[] _points = new Vector3[5];
        private Vector3[] _samplePoints = new Vector3[5];

        private HighPointList _pendingHighMaxes;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaycastBoxFivePointProvider"/> class.
        /// </summary>
        /// <param name="c">The collider.</param>
        public RaycastBoxFivePointProvider(Collider c)
        {
            Ensure.ArgumentNotNull(c, "c");
            var t = c.transform;

            var m = new ColliderMeasurement(c, false);
            var yoffset = -m.extents.y;
            _points[0] = m.center + new Vector3(0f, yoffset, 0f);
            _points[1] = m.center + new Vector3(m.extents.x, yoffset, -m.extents.z);
            _points[2] = m.center + new Vector3(-m.extents.x, yoffset, -m.extents.z);
            _points[3] = m.center + new Vector3(-m.extents.x, yoffset, m.extents.z);
            _points[4] = m.center + new Vector3(m.extents.x, yoffset, m.extents.z);

            _pendingHighMaxes = new HighPointList(2, t.TransformPoint(_points[3]));
        }

        /// <summary>
        /// Gets the height delta, i.e. the difference in height between where the unit will be at the end of the frame and the height the unit should aim to be at..
        /// </summary>
        /// <param name="input">The steering input</param>
        /// <returns>
        /// The height delta
        /// </returns>
        public float GetHeightDelta(SteeringInput input)
        {
            var unit = input.unit;

            //We need to sample at the position we predict we are going to be after this frame given the current velocity
            //If movement is vertical (or as good as) we want to look ahead a minimum distance
            var lookAhead = input.currentFullVelocity.OnlyXZ() * input.deltaTime;
            if (lookAhead.sqrMagnitude < 0.0001f && input.currentFullVelocity.y > 0f)
            {
                lookAhead = input.currentPlanarVelocity.normalized * 0.01f;
            }

            //Get the sample points
            var t = input.unit.transform;
            _samplePoints[0] = t.TransformPoint(_points[0]) + lookAhead;
            _samplePoints[1] = t.TransformPoint(_points[1]) + lookAhead;
            _samplePoints[2] = t.TransformPoint(_points[2]) + lookAhead;
            _samplePoints[3] = t.TransformPoint(_points[3]) + lookAhead;
            _samplePoints[4] = t.TransformPoint(_points[4]) + lookAhead;

            float baseY = _samplePoints[0].y;
            float maxClimb = unit.heightNavigationCapability.maxClimbHeight;
            float groundOffset = unit.groundOffset;

            //Do the height sampling
            var grid = input.grid;
            float rayStart = grid != null ? grid.cellMatrix.origin.y + grid.cellMatrix.upperBoundary : _samplePoints[0].y + maxClimb;
            float maxHeight = Consts.InfiniteDrop;

            RaycastHit hit;
            int highIdx = 0;
            Vector3 highNormal = Vector3.zero;
            for (int i = 0; i < 5; i++)
            {
                var point = _samplePoints[i];
                point.y = rayStart;

                if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, Layers.terrain))
                {
                    var sampledHeight = hit.point.y + groundOffset;
                    if (sampledHeight > maxHeight)
                    {
                        maxHeight = sampledHeight;
                        highNormal = hit.normal;
                        highIdx = i;
                    }

                    _samplePoints[i].y = sampledHeight;
                }
            }

            //When ascending there are situations where we need to continue the current rate of ascent even though the high point no longer dictates it.
            //This happens when moving from a slope onto a lesser slope or platform, we need to continue to ascend until the base is free otherwise the unit will collide with the terrain.
            var fhp = _samplePoints[3].y > _samplePoints[4].y ? _samplePoints[3] : _samplePoints[4];
            _pendingHighMaxes.RegisterHighpoint(fhp);

            if (highIdx < 3 && _pendingHighMaxes.count > 0 && _samplePoints[0].DirToXZ(_pendingHighMaxes.current).sqrMagnitude > _samplePoints[0].DirToXZ(_samplePoints[1]).sqrMagnitude)
            {
                _pendingHighMaxes.MoveNext();
            }

            if (_pendingHighMaxes.count > 0 && _pendingHighMaxes.currentHeight > maxHeight)
            {
                maxHeight = _pendingHighMaxes.currentHeight;
            }

            var delta = maxHeight - baseY;
            var slope = Vector3.Dot(Vector3.up, highNormal);
            var minSlope = Mathf.Cos(unit.heightNavigationCapability.maxSlopeAngle * Mathf.Deg2Rad);
            if (slope < minSlope && delta > maxClimb)
            {
                return 0f;
            }

            return delta;
        }
    }
}
