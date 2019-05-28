/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using System;
    using Apex.Common;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Height provider for sphere or vertical capsule colliders that samples three points, back, mid and front of the collider.
    /// </summary>
    public class RaycastSphericalThreePointProvider : IUnitHeightProvider
    {
        private Vector3[] _points = new Vector3[3];
        private Vector3[] _samplePoints = new Vector3[3];

        private float _radius;
        private HighPointList _pendingHighMaxes;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaycastSphericalThreePointProvider"/> class.
        /// </summary>
        /// <param name="c">The collider.</param>
        /// <exception cref="System.ArgumentException">A Spherical provider only works with sphere or vertical capsule colliders!</exception>
        public RaycastSphericalThreePointProvider(Collider c)
        {
            Ensure.ArgumentNotNull(c, "c");

            var t = c.transform;
            var scale = t.lossyScale;

            var sc = c as SphereCollider;
            var cc = c as CapsuleCollider;

            if (sc != null)
            {
                var scaling = Mathf.Max(scale.x, scale.y, scale.z);
                _radius = sc.radius * scaling;

                _points[0] = sc.center;
                _points[1] = sc.center + new Vector3(0f, 0f, sc.radius * scaling / scale.z);
                _points[2] = sc.center + new Vector3(0f, 0f, -sc.radius * scaling / scale.z);
            }
            else if (cc != null && cc.direction == 1)
            {
                var scaling = Mathf.Max(scale.x, scale.z);
                _radius = cc.radius * scaling;

                _points[0] = cc.center + new Vector3(0f, Mathf.Min((-cc.height * 0.5f) + (_radius / scale.y), 0f), 0f);
                _points[1] = cc.center + new Vector3(0f, 0f, cc.radius * scaling / scale.z);
                _points[2] = cc.center + new Vector3(0f, 0f, -cc.radius * scaling / scale.z);
            }
            else
            {
                throw new ArgumentException("A Spherical provider only works with sphere or vertical capsule colliders!");
            }

            _pendingHighMaxes = new HighPointList(2, t.TransformPoint(_points[0]));
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
            var center = _samplePoints[0] = t.TransformPoint(_points[0]) + lookAhead;
            _samplePoints[1] = t.TransformPoint(_points[1]) + lookAhead;
            _samplePoints[2] = t.TransformPoint(_points[2]) + lookAhead;

            float maxClimb = unit.heightNavigationCapability.maxClimbHeight;
            float groundOffset = unit.groundOffset;

            //Do the height sampling
            var grid = input.grid;
            float rayStart = grid != null ? grid.cellMatrix.origin.y + grid.cellMatrix.upperBoundary : center.y + maxClimb;
            float maxHeight = Consts.InfiniteDrop;

            RaycastHit hit;
            int highIdx = 0;
            Vector3 highNormal = Vector3.zero;
            for (int i = 0; i < 3; i++)
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
            _pendingHighMaxes.RegisterHighpoint(_samplePoints[1]);

            if (highIdx != 1 && _pendingHighMaxes.count > 0 && center.DirToXZ(_pendingHighMaxes.current).sqrMagnitude > _radius * _radius)
            {
                _pendingHighMaxes.MoveNext();
            }

            var hp = _samplePoints[highIdx];
            if (_pendingHighMaxes.count > 0 && (_pendingHighMaxes.currentHeight > maxHeight || (hp - center).sqrMagnitude > (_pendingHighMaxes.current - center).sqrMagnitude))
            {
                hp = _pendingHighMaxes.current;
            }
            else if (highIdx == 0)
            {
                //If the center is the highest point and we have reached the pinnacle of the ascent, then the center is in reality the highest point
                return maxHeight - (center.y - _radius);
            }

            //From the base point (on the ground) find the slope vector and the center vector
            var bp = _samplePoints[0];

            var slopeVector = (bp - hp).normalized;
            var centerVector = center - hp;

            //Find the closest point on the line and from that the closest point of the sphere periferi
            var dp = Vector3.Dot(centerVector, slopeVector);

            var closestPointLine = (dp > 0f) ? hp + (slopeVector * dp) : hp;
            var closestPoint = center + ((closestPointLine - center).normalized * _radius);

            //Find the plane normal for the plane represented by the slope line and an extra point (simply a point perpendicular to one of the other two at the same y)
            var pp3 = new Vector3(hp.z, hp.y, -hp.x);
            var pn = Vector3.Cross(pp3 - hp, hp - bp);

            //Get the closest y coordinate, that is the point where the sphere should rest, using the plane formula a(x-x0) + b(y-y0) + c(z-z0) = 0, where (a,b,c) is the normal vector and (x0 y0, z0) is a known point on the plane.
            //Since we know the x and z of the closest point we can find the proper y.
            var closestY = ((pn.x * (closestPoint.x - hp.x)) + (pn.z * (closestPoint.z - hp.z)) - (pn.y * hp.y)) / -pn.y;
            
            var delta = closestY - closestPoint.y;
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
