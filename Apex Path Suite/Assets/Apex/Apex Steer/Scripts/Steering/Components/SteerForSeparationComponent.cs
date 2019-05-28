namespace Apex.Steering.Components
{
    using System;
    using Apex.Utilities;
    using Common;
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to separate itself from any and all transient unit group members.
    /// </summary>
    [RequireComponent(typeof(SteeringScanner))]
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Separation", 1029)]
    [ApexComponent("Steering")]
    public class SteerForSeparationComponent : ArrivalBase
    {
        /// <summary>
        /// The maximum surrounding units to consider when calculating the separation vector. This is only honoured if the scanner sorts units by distance.
        /// </summary>
        [MinCheck(1, tooltip = "The maximum surrounding units to consider when calculating the separation vector. This is only honoured if the scanner sorts units by distance.")]
        public int maximumUnitsToConsider = 10;

        /// <summary>
        /// The distance at which the unit stops separating.
        /// </summary>
        [MinCheck(0.01f, label = "Separation Distance", tooltip = "The distance at which units are at rest with each other, i.e. if the distance between two units center + radius is less than this value, they will separate.")]
        public float separationDistance = 0.5f;

        /// <summary>
        /// What the force magnitude must be at minimum to not be ignored
        /// </summary>
        [MinCheck(0.0001f, label = "Minimum Force Magnitude", tooltip = "A factor used for ignoring separation vectors of less magnitude than this value.")]
        public float minimumForceMagnitude = 0.05f;

        /// <summary>
        /// How much force is applied on separation vectors - as a percentage of the maximum acceleration, when not swarming (arrival).
        /// </summary>
        [RangeX(0f, 1f, tooltip = "How much force is applied on separation vectors - as a percentage of the maximum acceleration, when not swarming (arrival).")]
        public float separationStrength = 0.25f;

        /// <summary>
        /// If true, the separation component automatically deactivates itself temporarily when there are any neighbouring cells that are blocked (this may prevent unwanted osccilation).
        /// </summary>
        [Tooltip("Controls the behaviour when the unit is next to blocked cells. Keeping full separation may result in unwanted oscillation.")]
        public SeparationMode blockedNeighboursBehaviour = SeparationMode.AvoidCollision;

        /// <summary>
        /// Whether to draw the separation vector as a gizmo in real-time.
        /// </summary>
        [Tooltip("If true, draws the last separation vector as a Gizmo.")]
        public bool drawGizmos = false;

        private Vector3 _lastSeparationVector;
        private SteeringScanner _scanner;

        [SerializeField, AttributeProperty("Ignored Units", "Defines which types of units are ignored by this unit in relation to separation")]
        private int _ignoredUnits;

        /// <summary>
        /// Represents different modes of separation
        /// </summary>
        public enum SeparationMode
        {
            /// <summary>
            /// Will make unit avoid collisions with others, but will not maintain full separation distance
            /// </summary>
            AvoidCollision,

            /// <summary>
            /// The maintain full separation distance
            /// </summary>
            MaintainFullSeparation,

            /// <summary>
            /// Will disable separation
            /// </summary>
            Disable
        }

        /// <summary>
        /// Gets the attribute mask that defines the attributes for which this unit will ignore other units.
        /// </summary>
        /// <value>
        /// The attribute mask with ignored unit types.
        /// </value>
        public AttributeMask ignoredUnits
        {
            get { return _ignoredUnits; }
            set { _ignoredUnits = value; }
        }

        /// <summary>
        /// Called on Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _scanner = this.GetComponent<SteeringScanner>();
            if (_scanner == null)
            {
                Debug.LogError(this.gameObject.name + " is missing its SteeringScanner component");
            }
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            _lastSeparationVector = Vector3.zero;

            var unit = input.unit;
            Vector3 selfPos = unit.position;
            var targetDistance = this.separationDistance;

            if (this.blockedNeighboursBehaviour != SeparationMode.MaintainFullSeparation && input.grid != null)
            {
                // only do this if on a valid grid
                var selfCell = input.grid.GetCell(selfPos, true);
                if (selfCell != null)
                {
                    // only do this if on a valid cell
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            if (x == 0 && z == 0)
                            {
                                continue;
                            }

                            // visit all neighbour cells ...
                            var neighbour = selfCell.GetNeighbour(x, z);
                            if (neighbour != null && !neighbour.IsWalkableFromWithClearance(selfCell, unit))
                            {
                                // if any neighbour cell is missing or unwalkable, return here and stop separating
                                if (this.blockedNeighboursBehaviour == SeparationMode.Disable)
                                {
                                    return;
                                }

                                targetDistance = 0f;

                                //Break out
                                z = x = 2;
                            }
                        }
                    }
                }
            }

            // prepare local variables
            Vector3 steeringVector = Vector3.zero;

            int avoidedCount = 0;
            var others = _scanner.units;
            int othersCount = _scanner.sortUnitsWithDistance ? Math.Min(others.count, this.maximumUnitsToConsider) : others.count;
            for (int i = 0; i < othersCount; i++)
            {
                var other = others[i];
                if (!other.isAlive)
                {
                    continue;
                }

                if ((_ignoredUnits & other.attributes) > 0)
                {
                    // other unit is found in the ignored attributes, so ignore it
                    continue;
                }

                if (!unit.hasArrivedAtDestination)
                {
                    // as long as this unit has not arrived...
                    if (unit.transientGroup == null || !object.ReferenceEquals(unit.transientGroup, other.transientGroup))
                    {
                        // ignore units in other groups 
                        continue;
                    }

                    if (other.hasArrivedAtDestination)
                    {
                        // ignore other units that have arrived
                        continue;
                    }
                }

                if (other.determination < unit.determination)
                {
                    // ignore units with less determination
                    continue;
                }

                Vector3 memberPos = other.position;
                Vector3 direction = (selfPos - memberPos);
                float combinedRadius = targetDistance + unit.radius + other.radius;
                if (direction.sqrMagnitude >= (combinedRadius * combinedRadius))
                {
                    // ignore any and all units that this unit does not overlap
                    continue;
                }

                // sum up separation vectors
                avoidedCount++;
                var dirMag = direction.magnitude;
                if (dirMag > 0f)
                {
                    steeringVector += (direction / dirMag) * (combinedRadius - dirMag);
                }
            }

            if (avoidedCount == 0)
            {
                // if no avoidance vectors were computed, then return early
                return;
            }

            steeringVector /= (float)avoidedCount;
            if (steeringVector.sqrMagnitude <= (minimumForceMagnitude * minimumForceMagnitude))
            {
                // if avoidance vector is too small, then return early
                return;
            }

            // Separation uses variable vector magnitudes
            // if the unit has arrived (swarming mode), use max acceleration, while unit is moving then use the separation strength factor as a percentage of the max acceleration
            float maxAcc = unit.hasArrivedAtDestination ? input.maxAcceleration : separationStrength * input.maxAcceleration;
            steeringVector = Seek(selfPos + steeringVector, input, maxAcc);

            _lastSeparationVector = steeringVector;

            output.desiredAcceleration = steeringVector;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            if (_lastSeparationVector.sqrMagnitude == 0f)
            {
                return;
            }

            Vector3 pos = this.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + _lastSeparationVector);
        }
    }
}