/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using System;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A precise height provider for capsules.
    /// </summary>
    public class CapsuleCastProvider : IUnitHeightProvider
    {
        private float _radius;

        //Represents the y offset from the unit's position to the center of the bottom sphere (or just to the collider center for x and z direction colliders)
        private float _centerOffsetY;
        private Vector3 _toEndPointOne;
        private Vector3 _toEndPointTwo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapsuleCastProvider"/> class.
        /// </summary>
        /// <param name="c">The collider.</param>
        /// <exception cref="System.ArgumentException">A capsule cast provider only works with capsule colliders (surprise)!</exception>
        public CapsuleCastProvider(Collider c)
        {
            Ensure.ArgumentNotNull(c, "c");

            var cc = c as CapsuleCollider;
            if (cc == null)
            {
                throw new ArgumentException("A capsule cast provider only works with capsule colliders (surprise)!");
            }

            var t = c.transform;
            var scale = t.lossyScale;
            _radius = cc.radius * Mathf.Max(scale.x, scale.z);

            var midToEndCenter = Mathf.Max((cc.height * scale.y * 0.5f) - _radius, 0f);
            _toEndPointOne = _toEndPointTwo = new Vector3(cc.center.x * scale.x, cc.center.y * scale.y, cc.center.y * scale.y);

            switch (cc.direction)
            {
                //x-axis aligned
                case 0:
                {
                    _toEndPointOne.x -= midToEndCenter;
                    _toEndPointTwo.x += midToEndCenter;
                    _centerOffsetY = cc.center.y * scale.y;
                    break;
                }

                //y-axis aligned
                case 1:
                {
                    _toEndPointOne.y -= midToEndCenter;
                    _toEndPointTwo.y += midToEndCenter;
                    _centerOffsetY = (cc.center.y * scale.y) - midToEndCenter;

                    break;
                }

                //z-axis aligned
                case 2:
                {
                    _toEndPointOne.z -= midToEndCenter;
                    _toEndPointTwo.z += midToEndCenter;
                    _centerOffsetY = cc.center.y * scale.y;
                    break;
                }
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
            var baseY = unit.basePosition.y;
            var maxClimb = unit.heightNavigationCapability.maxClimbHeight;

            //Put the start at the top of the grid bounds or at the unit's head or max climb above its center, depending on whether we are on a grid or not
            var grid = input.grid;
            var matrix = grid != null ? grid.cellMatrix : null;
            var startY = matrix != null ? matrix.origin.y + matrix.upperBoundary : baseY + Mathf.Max(unit.height, _radius + maxClimb);

            //We need to sample at the position we predict we are going to be after this frame given the current velocity
            //If movement is vertical (or as good as) we want to look ahead a minimum distance
            var lookAhead = input.currentFullVelocity.OnlyXZ() * input.deltaTime;
            if (lookAhead.sqrMagnitude < 0.0001f && input.currentFullVelocity.y > 0f)
            {
                lookAhead = input.currentPlanarVelocity.normalized * 0.01f;
            }

            var endOne = unit.position + _toEndPointOne + lookAhead;
            var endTwo = unit.position + _toEndPointTwo + lookAhead;
            endOne.y = endTwo.y = startY;

            RaycastHit hit;
            if (!Physics.CapsuleCast(endOne, endTwo, _radius, Vector3.down, out hit, Mathf.Infinity, Layers.terrain))
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
