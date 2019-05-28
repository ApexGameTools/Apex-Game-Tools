/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using Apex.LoadBalancing;
    using Apex.Steering.VectorFields;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// This steering component makes the attached unit follow a given formation set on its group - if it has been set.
    /// SteerForFormation handles cell sampling in multiple directions (towards group and towards formation position) in order to check when the formation position can be allowed, and when it should be dropped.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Formation", 1025)]
    [ApexComponent("Steering")]
    public class SteerForFormationComponent : ArrivalBase, ILoadBalanced
    {
        /// <summary>
        /// If true, draws debug information including the desired formation position and the formation indices.
        /// </summary>
        [Tooltip("If true, draws debug information including the desired formation position and the formation indices.")]
        public bool debugDraw = false;

        /// <summary>
        /// How many cells ahead the formation samples. If any sampled cells are blocked, the formation is dropped temporarily.
        /// </summary>
        [MinCheck(0, label = "Sampled Cell Count", tooltip = "How many cells ahead the formation samples. If any sampled cells are blocked, the formation is dropped temporarily.")]
        public int sampledCellCount = 2;

        /// <summary>
        /// How often cell sampling is done, load balanced, towards the formation position and towards the model unit.
        /// </summary>
        [MinCheck(0.01f, label = "Sampling Update Interval", tooltip = "How often cell sampling is done, load balanced, towards the formation position and towards the model unit.")]
        public float samplingUpdateInterval = 0.25f;

        /// <summary>
        /// The maximum distance at which this unit still responds to its formation. Outside of this radius (from the group) the formation is ignored.
        /// </summary>
        [MinCheck(1f, label = "Max Formation Radius", tooltip = "The maximum distance at which this unit still responds to its formation. Outside of this radius (from the group) the formation is ignored.")]
        public float maxFormationRadius = 40f;

        /// <summary>
        /// Controls whether the unit will drop its formation on arrival and instead allow other steering components to control the unit's behaviour.
        /// </summary>
        [Tooltip("Controls whether the unit will drop its formation on arrival and instead allow other steering components to control the unit's behaviour.")]
        public bool dropFormationOnArrival = false;

        private IUnitFacade _unitData;
        private bool _stopped;
        private IGrid _grid;
        private IPositioned _formationPos;

        /// <summary>
        /// Gets a value indicating whether to repeatedly update this entity each interval.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity should be updated each interval; <c>false</c> if it should only be updated once and then removed from the load balancer.
        /// </value>
        public bool repeat
        {
            get { return this.enabled; }
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            base.OnStartAndEnable();

            // add this component to the load balancer with the desired update interval
            NavLoadBalancer.steering.Add(this, samplingUpdateInterval, true);
        }

        /// <summary>
        /// Called when disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            // remove this component from the load balancer
            NavLoadBalancer.steering.Remove(this);
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            // Set the formation position to null to make sure that it is invalid when it's supposed to be
            input.unit.formationPos = null;
            _formationPos = null;

            // set instance variables to be used in load balanced update method
            _unitData = input.unit;
            _grid = input.grid;

            if (_grid == null)
            {
                // if not on a grid, drop formation
                return;
            }

            // must cast the group to have access to GetFormationPosition
            var group = _unitData.transientGroup as DefaultSteeringTransientUnitGroup;
            if (group == null)
            {
                // if not in a group, drop formation
                return;
            }

            int count = group.count;
            if (count == 0)
            {
                // if there are no members in the group, drop formation
                return;
            }

            var modelUnit = group.modelUnit;
            if (modelUnit == null)
            {
                // if group's model unit is missing, then drop formation
                return;
            }

            if (_unitData.formationIndex < 0)
            {
                // if this unit has not been given a valid formation index, then drop formation
                return;
            }

            if (_unitData.hasArrivedAtDestination && this.dropFormationOnArrival)
            {
                return;
            }

            _formationPos = group.GetFormationPosition(_unitData.formationIndex);
            if (_formationPos == null || _stopped)
            {
                // if there is no valid formation position calculated or it is invalid currently, then drop formation
                return;
            }

            // make sure desiredSpeed does not spill over or under
            var desiredSpeed = Mathf.Min(1.5f * input.desiredSpeed, _unitData.maximumSpeed);

            _unitData.formationPos = _formationPos;
            Vector3 arrivalVector = Arrive(_formationPos.position, input, desiredSpeed);
            if (modelUnit.hasArrivedAtDestination && this.hasArrived)
            {
                // When the unit arrives at the designated formation position, make sure hasArrivedAtDestination becomes true
                // Also, must return here, to avoid outputting a result
                _unitData.hasArrivedAtDestination = true;
                return;
            }

            output.maxAllowedSpeed = desiredSpeed;
            output.desiredAcceleration = arrivalVector;
        }

        /// <summary>
        /// Executes the update.
        /// </summary>
        /// <param name="deltaTime">The delta time, i.e. the time passed since the last update.</param>
        /// <param name="nextInterval">The time that will pass until the next update.</param>
        /// <returns>
        /// Can return the next interval by which the update should run. To use the default interval return null.
        /// </returns>
        public float? ExecuteUpdate(float deltaTime, float nextInterval)
        {
            if (_unitData == null || _unitData.transientGroup == null || _grid == null || _formationPos == null)
            {
                // if any data is missing, stop now
                _stopped = true;
                return null;
            }

            // prepare local variables
            var group = _unitData.transientGroup;
            var modelUnit = group.modelUnit;
            if (modelUnit == null || !modelUnit.isAlive)
            {
                _stopped = true;
                return null;
            }

            Vector3 unitPos = _unitData.position;
            Vector3 modelUnitPos = modelUnit.position;
            Vector3 vectorUnitToModelUnit = (modelUnitPos - unitPos);

            if (vectorUnitToModelUnit.sqrMagnitude > (maxFormationRadius * maxFormationRadius))
            {
                // if we are outside the maximum formation radius, then drop formation
                _stopped = true;
                return null;
            }

            var selfCell = _grid.GetCell(unitPos, false);
            if (selfCell == null || !selfCell.IsWalkableWithClearance(_unitData))
            {
                // if not standing on a valid, walkable cell, drop formation
                _stopped = true;
                return null;
            }

            // formation position cell sampling
            var formationCell = _grid.GetCell(_formationPos.position, false);
            if (formationCell == null || !formationCell.IsWalkableFromWithClearance(selfCell, _unitData))
            {
                // if the cell at the formation position is missing or blocked, then drop formation
                _stopped = true;
                return null;
            }

            float cellSize = _grid.cellSize;

            // cell sampling towards model unit / group
            var lastCell = selfCell;
            float vectorToModelUnitMagnitude = vectorUnitToModelUnit.magnitude;
            Vector3 vectorToModelUnitNormal = vectorUnitToModelUnit / vectorToModelUnitMagnitude;
            int steps = Mathf.CeilToInt(vectorToModelUnitMagnitude / cellSize);
            for (int j = 1; j <= steps; j++)
            {
                // start loop at 1, don't check the cell that the unit is standing on (we assume it's valid)
                float stepSize = j * cellSize;
                Vector3 sampleVector = unitPos + (vectorToModelUnitNormal * stepSize);

                var cell = _grid.GetCell(sampleVector, false);
                if (cell == null || !cell.IsWalkableFromWithClearance(lastCell, _unitData))
                {
                    // if a cell between this unit and its model unit / group leader is missing or blocked, then drop formation
                    _stopped = true;
                    return null;
                }

                lastCell = cell;
            }

            // cell sampling
            Vector3 vectorToFormation = (_formationPos.position - unitPos);
            Vector3 vectorToFormationNormal = vectorToFormation.normalized;
            float max = vectorToFormation.magnitude + _unitData.radius;

            for (int i = 1; i <= sampledCellCount && (i * cellSize) < max; i++)
            {
                // start loop at 1, don't check the cell that the unit is standing on (we assume it's valid)
                float stepSize = i * cellSize;

                Vector3 samplePos = unitPos + (vectorToFormationNormal * stepSize);

                var cell = _grid.GetCell(samplePos, false);
                if (cell == null || !cell.IsWalkableFromWithClearance(selfCell, _unitData))
                {
                    // if a cell between this unit and the designated formation position cell is missing or blocked, drop formation..
                    _stopped = true;
                    return null;
                }
            }

            // check for portal cells
            var steerGrp = group as DefaultSteeringTransientUnitGroup;
            if (steerGrp != null && steerGrp.vectorField != null)
            {
                // if the group has a vector field, check if standing on a portal cell
                VectorFieldCell fieldCell = steerGrp.vectorField.GetFieldCellAtPos(selfCell);
                if (fieldCell.pathPortalIndex > 0)
                {
                    // if standing on a portal cell, drop formation
                    _stopped = true;
                    return null;
                }
            }

            // if we get to here, all is well - allow formation
            _stopped = false;
            return null;
        }

        private void OnDrawGizmos()
        {
            if (!debugDraw)
            {
                return;
            }

            if (_unitData == null || _unitData.formationPos == null)
            {
                return;
            }

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_unitData.formationPos.position, _unitData.radius);

            var group = _unitData.transientGroup as DefaultSteeringTransientUnitGroup;
            if (group == null)
            {
                return;
            }

            var finalFormPos = group.GetFinalFormationPosition(_unitData.formationIndex);
            if (finalFormPos == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(finalFormPos.position, _unitData.radius);

            if (_unitData.hasArrivedAtDestination)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(_unitData.position, _unitData.radius);
            }
        }

        //For internal debugging
        //private void OnGUI()
        //{
        //    if (!debugDraw)
        //    {
        //        return;
        //    }

        //    if (Camera.current == null || _unitData == null || transform == null)
        //    {
        //        return;
        //    }

        //    Vector3 pos = Camera.current.WorldToScreenPoint(transform.position);
        //    pos.y = Screen.height - pos.y;
        //    GUI.backgroundColor = Color.white;
        //    GUI.color = Color.black;
        //    GUI.Label(new Rect(pos.x, pos.y, 50f, 20f), _unitData.formationIndex.ToString());

        //    if (_unitData.formationPos == null)
        //    {
        //        return;
        //    }

        //    pos = Camera.current.WorldToScreenPoint(_unitData.formationPos.position);
        //    pos.y = Screen.height - pos.y;
        //    GUI.color = Color.black;
        //    GUI.Label(new Rect(pos.x, pos.y, 50f, 20f), _unitData.formationIndex.ToString());
        //}
    }
}