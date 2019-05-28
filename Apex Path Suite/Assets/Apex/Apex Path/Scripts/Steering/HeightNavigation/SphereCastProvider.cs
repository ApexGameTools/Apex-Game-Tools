/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using System;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Precise height provider for sphere or vertical capsule colliders.
    /// </summary>
    public class SphereCastProvider : IUnitHeightProvider
    {
        private float _radius;
        private float _centerOffsetY;

        /// <summary>
        /// Initializes a new instance of the <see cref="SphereCastProvider"/> class.
        /// </summary>
        /// <param name="c">The collider.</param>
        /// <exception cref="System.ArgumentException">A sphere cast provider only works with sphere or vertical capsule colliders!</exception>
        public SphereCastProvider(Collider c)
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
                _centerOffsetY = sc.center.y * scaling;
            }
            else if (cc != null && cc.direction == 1)
            {
                _radius = cc.radius * Mathf.Max(scale.x, scale.z);
                _centerOffsetY = (cc.center.y * scale.y) - Mathf.Max((cc.height * scale.y * 0.5f) - _radius, 0f);
            }
            else
            {
                throw new ArgumentException("A sphere cast provider only works with sphere or vertical capsule colliders!");
            }
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
            var start = unit.basePosition;
            var baseY = start.y;
            var maxClimb = unit.heightNavigationCapability.maxClimbHeight;

            //Put the start at the top of the grid bounds or at the unit's head or max climb above its center, depending on whether we are on a grid or not
            var grid = input.grid;
            var matrix = grid != null ? grid.cellMatrix : null;
            start.y = matrix != null ? matrix.origin.y + matrix.upperBoundary : start.y + Mathf.Max(unit.height, _radius + maxClimb);

            //We need to sample at the position we predict we are going to be after this frame given the current velocity
            //If movement is vertical (or as good as) we want to look ahead a minimum distance
            var lookAhead = input.currentFullVelocity.OnlyXZ() * input.deltaTime;
            if (lookAhead.sqrMagnitude < 0.0001f && input.currentFullVelocity.y > 0f)
            {
                lookAhead = input.currentPlanarVelocity.normalized * 0.01f;
            }

            RaycastHit hit;
            if (!Physics.SphereCast(start + lookAhead, _radius, Vector3.down, out hit, Mathf.Infinity, Layers.terrain))
            {
                return Consts.InfiniteDrop;
            }
            
            //Make sure the height difference is not greater than what the unit can climb
            var diff = hit.point.y - baseY;
            var slope = Vector3.Dot(Vector3.up, hit.normal);
            var minSlope = Mathf.Cos(unit.heightNavigationCapability.maxSlopeAngle * Mathf.Deg2Rad);
            if (slope > minSlope || diff <= maxClimb)
            {
                //Get the target position we want to be at (circle center)
                var center = hit.point + (hit.normal * _radius);
                return center.y - (unit.position.y + _centerOffsetY) + unit.groundOffset;
            }
            
            //If the height ahead is too high, we just wanna stay at the height we are at.
            return 0f;
        }
    }
}
