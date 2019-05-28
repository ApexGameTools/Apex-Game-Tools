/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.Follow
{
    using Apex;
    using Apex.Units;
    using UnityEngine;

    public class FollowSimple : ExtendedMonoBehaviour
    {
        public Transform target;

        private const float MinimalMoveDiff = 0.5f;
        private Vector3 _lastRecordedTargetPosition;
        private IUnitFacade _unit;

        private void Awake()
        {
            _unit = this.GetUnitFacade();
        }

        protected override void OnStartAndEnable()
        {
            _lastRecordedTargetPosition = target.position;
            _unit.MoveTo(_lastRecordedTargetPosition, false);
        }

        private void Update()
        {
            bool updateMoveOrder = (_lastRecordedTargetPosition - target.position).sqrMagnitude > MinimalMoveDiff;

            if (updateMoveOrder)
            {
                _lastRecordedTargetPosition = target.position;
                _unit.MoveTo(_lastRecordedTargetPosition, false);
            }
        }
    }
}
