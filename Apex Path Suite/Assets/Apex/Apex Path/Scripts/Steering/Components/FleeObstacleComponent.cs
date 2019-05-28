/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using Apex;
    using Apex.Messages;
    using Apex.Services;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A steering component that will cause the unit to move away from obstacles that encroach on its position.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer to flee obstacle", 1035)]
    [ApexComponent("Steering")]
    public class FleeObstacleComponent : SteeringComponent
    {
        /// <summary>
        /// The maximum cell radius to look for a new position
        /// </summary>
        public int fleeMaxRadius = 5;

        private Cell _targetCell;

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            var grid = input.grid;
            if (grid == null)
            {
                _targetCell = null;
                return;
            }

            var unit = input.unit;

            var pos = unit.position;
            var cell = grid.GetCell(pos, true);
            if (cell.IsWalkableWithClearance(unit))
            {
                if (_targetCell == null)
                {
                    return;
                }

                var dir = pos.DirToXZ(cell.position);
                var distanceTreshold = (grid.cellSize / 2f) - unit.radius;

                if (dir.sqrMagnitude > distanceTreshold * distanceTreshold)
                {
                    _targetCell = cell;
                }
                else
                {
                    _targetCell = null;
                    return;
                }
            }
            else if (_targetCell == null || _targetCell == cell)
            {
                _targetCell = grid.GetNearestWalkableCell(pos, pos, true, this.fleeMaxRadius, unit);
            }

            output.desiredAcceleration = Seek(_targetCell.position, input);
            output.maxAllowedSpeed = input.unit.maximumSpeed;
        }
    }
}
