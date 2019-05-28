#pragma warning disable 1591

namespace Apex.Examples.Steering
{
    using System.Collections;
    using Apex.Units;
    using UnityEngine;
    using WorldGeometry;

    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Apex/Examples/Make Steer Group and Move", 1010)]
    public class SteerMakeGroupAndMove : ExtendedMonoBehaviour
    {
        public Vector3 moveToTarget;

        public float groupCreationTime = 1f;

        private int _groupCount = 0;
        private bool _moveOnce = false;

        private DefaultSteeringTransientUnitGroup _group;
        private float _startTime = 0f;

        protected override void OnEnable()
        {
            base.OnEnable();

            _startTime = Time.time;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Layers.InLayer(other.gameObject, Layers.units))
            {
                return;
            }

            var unit = other.GetUnitFacade();
            if (unit == null)
            {
                return;
            }

            var grp = unit.transientGroup as DefaultSteeringTransientUnitGroup;

            float currentTime = Time.time;
            if (currentTime - _startTime < this.groupCreationTime)
            {
                if (grp == null)
                {
                    if (_group == null)
                    {
                        _group = new DefaultSteeringTransientUnitGroup(1);
                    }

                    _group.Add(unit);
                    StartCoroutine(DelayedMove());
                }
            }
            else
            {
                if (grp != null)
                {
                    if (++_groupCount >= grp.count + 1)
                    {
                        grp.MoveTo(moveToTarget, false);
                        _groupCount = 0;
                    }
                }
            }
        }

        private IEnumerator DelayedMove()
        {
            yield return new WaitForSeconds(this.groupCreationTime);

            if (!_moveOnce)
            {
                _moveOnce = true;
                _group.MoveTo(moveToTarget, false);
            }
        }
    }
}