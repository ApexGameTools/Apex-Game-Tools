/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using Apex.Services;
    using Apex.Units;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// A steering component that enables the unit to be harder to push into heights or drops that it cannot scale, as well as leaving the grid (unless for another grid)
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steer for Containment", 1022)]
    [ApexComponent("Steering")]
    public class SteerForContainmentComponent : SteeringComponent
    {
        /// <summary>
        /// The distance used for sampling height in all 4 directions of the unit, e.g. how many meters away from the unit it samples.
        /// Note that this value is added to the unit's radius, so small values are recommended.
        /// </summary>
        [MinCheck(0.1f, label = "Buffer Distance", tooltip = "How far away in meters, from the unit, it height samples in 4 directions. Note that this value is added to the unit's radius, so small values are recommended")]
        public float bufferDistance = 0.3f;

        /// <summary>
        /// If true, draws debug information, including the latest containment vector.
        /// </summary>
        [Tooltip("If true, draws debug information, including the latest containment vector.")]
        public bool drawGizmos = false;

        private ISampleHeights _heightSampler;
        private Vector3 _lastContainVector;
        private IUnitFacade _unitData;
        private HeightNavigationCapabilities _heightCaps;

        /// <summary>
        /// Called on Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            // cache a few properties
            _heightSampler = GameServices.heightStrategy.heightSampler;

            var unitData = this.GetUnitFacade();
            _heightCaps = unitData.heightNavigationCapability;
            bufferDistance += unitData.radius;
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            _lastContainVector = Vector3.zero;

            if (input.grid == null)
            {
                // if off-grid, exit early
                return;
            }

            _unitData = input.unit;
            if (!_unitData.isGrounded)
            {
                // while in the air, exit early
                return;
            }

            // prepare local variables
            Vector3 selfPos = _unitData.position;
            Vector3 containVector = Vector3.zero;
            float selfHeight = _unitData.basePosition.y;

            // generate positions to the left and right
            Vector3 rightPos = selfPos + (Vector3.right * bufferDistance);
            Vector3 leftPos = selfPos + (Vector3.left * bufferDistance);
            float rightHeight, leftHeight;

            // sample left and right
            bool rightContain = _heightSampler.TrySampleHeight(rightPos, out rightHeight);
            bool leftContain = _heightSampler.TrySampleHeight(leftPos, out leftHeight);

            // if cell is missing or drop is more than allowed drop height or climb is more than allowed climb height, then compute an axis-aligned containment vector
            if (!rightContain || (selfHeight - rightHeight > _heightCaps.maxDropHeight) || (rightHeight - selfHeight > _heightCaps.maxClimbHeight))
            {
                containVector = Vector3.left;
            }
            else if (!leftContain || (selfHeight - leftHeight > _heightCaps.maxDropHeight) || (leftHeight - selfHeight > _heightCaps.maxClimbHeight))
            {
                containVector = Vector3.right;
            }

            // generate positions forward and backwards
            Vector3 forwardPos = selfPos + (Vector3.forward * bufferDistance);
            Vector3 backwardPos = selfPos + (Vector3.back * bufferDistance);
            float forwardHeight, backHeight;

            // sample forward and backwards
            bool forwardContain = _heightSampler.TrySampleHeight(forwardPos, out forwardHeight);
            bool backContain = _heightSampler.TrySampleHeight(backwardPos, out backHeight);

            // if cell is missing or drop is more than allowed drop height or climb is more than allowed climb height, then compute an axis-aligned containment vector
            if (!forwardContain || (selfHeight - forwardHeight > _heightCaps.maxDropHeight) || (forwardHeight - selfHeight > _heightCaps.maxClimbHeight))
            {
                // we need to check whether containVector has a value beforehand, in which case we need to normalize
                containVector = containVector.sqrMagnitude != 0f ? (containVector + Vector3.back).normalized : Vector3.back;
            }
            else if (!backContain || (selfHeight - backHeight > _heightCaps.maxDropHeight) || (backHeight - selfHeight > _heightCaps.maxClimbHeight))
            {
                // we need to check whether containVector has a value beforehand, in which case we need to normalize
                containVector = containVector.sqrMagnitude != 0f ? (containVector + Vector3.forward).normalized : Vector3.forward;
            }

            if (containVector.sqrMagnitude == 0f)
            {
                // no contain vectors to worry about - no containment necessary
                return;
            }

            // Containment vectors are always "full strength"
            Vector3 steeringVector = containVector * input.maxAcceleration;
            _lastContainVector = steeringVector;
            output.desiredAcceleration = steeringVector;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            if (_lastContainVector.sqrMagnitude != 0f)
            {
                return;
            }

            Vector3 pos = _unitData.position;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + _lastContainVector);
        }
    }
}