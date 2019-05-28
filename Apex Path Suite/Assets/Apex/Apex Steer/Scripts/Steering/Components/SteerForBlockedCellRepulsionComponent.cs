/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using Apex.DataStructures;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to be repelled by blocked cells, i.e. it is able to better avoid blocked cells by being repelled from them
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Blocked Cell Repulsion", 1021)]
    [ApexComponent("Steering")]
    public class SteerForBlockedCellRepulsionComponent : SteeringComponent
    {
        /// <summary>
        /// A little margin added to the unit's radius as a buffer
        /// </summary>
        [MinCheck(0.01f, label = "Radius Margin", tooltip = "A margin added to the unit's radius when it considers whether it overlaps with a blocked cell")]
        public float radiusMargin = 0.1f;

        /// <summary>
        /// The repulsion strength used for how much repulsion from blocked cells is desired
        /// </summary>
        [MinCheck(1f, label = "Repulsion Strength", tooltip = "A factor used for multiplying the repulsion vector, i.e. this factor explicitely sets the repulsion vector magnitude")]
        public float repulsionStrength = 200f;

        /// <summary>
        /// If true, draws debug information, including the last cell repulsion vector.
        /// </summary>
        [Tooltip("If true, draws debug information, including the last cell repulsion vector.")]
        public bool drawGizmos = false;

        private Vector3 _lastAvoidVector;
        private IUnitFacade _unitData;
        private DynamicArray<Cell> _neighbours;

        /// <summary>
        /// Called on Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _neighbours = new DynamicArray<Cell>(4);
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            _lastAvoidVector = Vector3.zero;

            var grid = input.grid;
            if (grid == null)
            {
                // if no grid is available, return
                return;
            }

            _unitData = input.unit;
            if (!_unitData.isGrounded)
            {
                // in the air, give up
                return;
            }

            Vector3 selfPos = _unitData.position;
            var selfCell = grid.GetCell(selfPos);
            if (selfCell == null || !selfCell.IsWalkableWithClearance(_unitData))
            {
                // already within a blocked or missing cell, give up
                return;
            }

            // Find and avoid 4 neighbour cells - if they are blocked
            _neighbours.Clear();
            _neighbours.Add(selfCell.GetNeighbour(-1, 0));
            _neighbours.Add(selfCell.GetNeighbour(0, -1));
            _neighbours.Add(selfCell.GetNeighbour(1, 0));
            _neighbours.Add(selfCell.GetNeighbour(0, 1));

            // the total radius is the unit's radius + the radius margin + half of the current grid's cell size
            float selfRadius = _unitData.radius + radiusMargin + (grid.cellSize / 2f);
            float selfRadiusSqr = selfRadius * selfRadius;
            Vector3 avoidVector = Vector3.zero;

            int neighboursCount = _neighbours.count;
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbourCell = _neighbours[i];
                if (neighbourCell == null)
                {
                    // ignore missing cells, this is not containment
                    continue;
                }

                var dir = neighbourCell.position.DirToXZ(selfPos);
                float distanceSqr = dir.sqrMagnitude;
                if (distanceSqr < selfRadiusSqr && !neighbourCell.IsWalkableFromWithClearance(selfCell, _unitData))
                {
                    // sum up a vector comprising of all avoid vectors
                    avoidVector += dir;
                }
            }

            if (avoidVector.sqrMagnitude == 0f)
            {
                return;
            }

            // set the repulsion vector's magnitude to repulsionStrength
            Vector3 steeringVector = avoidVector.normalized * repulsionStrength;
            _lastAvoidVector = steeringVector;
            output.desiredAcceleration = steeringVector;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            if (_lastAvoidVector.sqrMagnitude == 0f)
            {
                return;
            }

            Vector3 pos = _unitData.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + _lastAvoidVector);
        }
    }
}