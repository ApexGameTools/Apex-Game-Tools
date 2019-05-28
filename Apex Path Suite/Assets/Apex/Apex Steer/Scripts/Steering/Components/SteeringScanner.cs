/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.Services;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A steering scanner used by steering components in order to be able to perceive other units and obstacles in the 'blocks' layer
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steering Scanner", 1038)]
    [ApexComponent("Steering")]
    public class SteeringScanner : ExtendedMonoBehaviour, ILoadBalanced, ISupportRuntimeStateChange
    {
        /// <summary>
        /// The time in seconds between scanner scans (update frequency)
        /// </summary>
        [MinCheck(0.1f, label = "Scan Interval", tooltip = "How often the load balancer executes this scanner. Lower values grant better behaviour, but higher values grant better performance")]
        public float scanInterval = 0.2f;

        /// <summary>
        /// The scan radius - the actual radius defining the 'circle of perception'
        /// </summary>
        [MinCheck(1f, label = "Scan Radius", tooltip = "How large a radius this scanner scans units in, i.e. how far away other units and blocks can be and still be perceived. Higher values grant better behaviour, lower values better performance.")]
        public float scanRadius = 9.0f;

        /// <summary>
        /// The distance with which the unit's position is projected forward (0 means current time, 1 means one meter in front of the unit)
        /// This distance is always capped by scanRadius.
        /// </summary>
        [Label("Forecast Distance", "If not 0, the position used as origin for the overlap sphere cast is projected in velocity direction by this magnitude. This distance is always capped by scanRadius.")]
        public float forecastDistance = 1f;

        /// <summary>
        /// If true, the scanner sorts all scanned units according to their distance to this unit (nearest first).
        /// Must be true if used in conjunction with <see cref="SteerForUnitAvoidanceComponent" /> without accummulating avoid vectors.
        /// </summary>
        [Label("Sort Units With Distance", "If true, the scanner sorts all scanned units according to their distance to this unit (nearest first). Must be true if used in conjunction with UnitAvoidance without accumulating avoid vectors.")]
        public bool sortUnitsWithDistance = true;

        /// <summary>
        /// If true, the scanner does not return units in the same transient group as this unit, if false returns all units within scanRadius.
        /// </summary>
        [Label("Filter Away Units in Same Group", "If true, the scanner does not return units in the same transient group as this unit, if false returns all units within scanRadius.")]
        public bool filterAwayUnitsInSameGroup = false;

        private Collider[] _blocks = new Collider[0];

        private DynamicArray<IUnitFacade> _units;
        private IUnitFacade _unitData;
        private DistanceComparer<IUnitFacade> _unitComparer = new DistanceComparer<IUnitFacade>(true);

        /// <summary>
        /// Gets a list of all the units scanned.
        /// </summary>
        public IIterable<IUnitFacade> units
        {
            get { return _units; }
        }

        /// <summary>
        /// Gets the colliders of all scanned blocks.
        /// </summary>
        public Collider[] blocks
        {
            get { return _blocks; }
        }

        /// <summary>
        /// Whether to repeat being called load balanced.
        /// </summary>
        bool ILoadBalanced.repeat
        {
            get { return this.enabled; }
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            NavLoadBalancer.scanners.Add(this, this.scanInterval, false);

            _unitData = this.GetUnitFacade();
            _units = new DynamicArray<IUnitFacade>(5);

            if (this.forecastDistance > this.scanRadius)
            {
                // forecast distance can never surpass the scan radius
                this.forecastDistance = this.scanRadius;
            }
        }

        private void OnDisable()
        {
            NavLoadBalancer.scanners.Remove(this);
        }

        /// <summary>
        /// Executes the update.
        /// </summary>
        /// <param name="deltaTime">The delta time, i.e. the time passed since the last update.</param>
        /// <param name="nextInterval">The time that will pass until the next update.</param>
        /// <returns>
        /// Can return the next interval by which the update should run. To use the default interval return null.
        /// </returns>
        float? ILoadBalanced.ExecuteUpdate(float deltaTime, float nextInterval)
        {
            _units.Clear();

            Vector3 currentPos = _unitData.position;
            // only project the position if forecastDistance is not 0
            Vector3 projPos = this.forecastDistance != 0f ? currentPos + (_unitData.velocity.normalized * this.forecastDistance) : currentPos;

            // get all colliders with 'blocks' layer
            _blocks = UnityServices.physics.OverlapSphere(projPos, this.scanRadius, Layers.blocks);

            // get all colliders with 'units' layer
            var units = UnityServices.physics.OverlapSphere(projPos, this.scanRadius, Layers.units);
            int unitsCount = units.Length;
            if (unitsCount == 0)
            {
                // if there are no scanned units, then just return early
                return null;
            }

            float unitBaseY = _unitData.basePosition.y;

            var unitManager = GameServices.gameStateManager;

            for (int i = 0; i < unitsCount; i++)
            {
                Collider other = units[i];
                if (other == null || other.Equals(_unitData.collider))
                {
                    // make sure to null out units with a missing collider OR units that match itself (this unit)
                    continue;
                }

                var otherData = unitManager.GetUnitFacade(other.gameObject, true);
                float otherBaseY = otherData.basePosition.y;
                if (unitBaseY + _unitData.height < otherBaseY || unitBaseY > otherBaseY + otherData.height)
                {
                    // ignore if the other unit is at a different elevation
                    continue;
                }

                if (this.filterAwayUnitsInSameGroup)
                {
                    var otherGroup = otherData.transientGroup;
                    if (otherGroup != null && object.ReferenceEquals(otherGroup, _unitData.transientGroup))
                    {
                        // filter away units that are in the same transient unit group as this unit (optionally)
                        continue;
                    }
                }

                // if we get to here, add the scanned unit's UnitFacade to the list
                _units.Add(otherData);
            }

            if (this.sortUnitsWithDistance)
            {
                // sort identified units depending on their distance to this unit (nearest first)
                _unitComparer.compareTo = currentPos;
                _units.Sort(_unitComparer);
            }

            return null;
        }

        /// <summary>
        /// Reevaluates the state.
        /// </summary>
        public void ReevaluateState()
        {
            if (this.forecastDistance > this.scanRadius)
            {
                // forecast distance can never surpass the scan radius
                this.forecastDistance = this.scanRadius;
            }
        }
    }
}