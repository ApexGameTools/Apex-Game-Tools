#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using System;
    using Apex.LoadBalancing;
    using Apex.PathFinding.MoveCost;
    using Apex.Services;
    using Apex.Steering;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Example implementation of a Portal Action
    /// </summary>
    [AddComponentMenu("Apex/Examples/Extensibility/Portal Action Jump", 1007)]
    public class PortalActionJumpComponent : MonoBehaviour, IPortalAction
    {
        /// <summary>
        /// How high should it jump
        /// </summary>
        public float heightFactor = 0.3f;

        /// <summary>
        /// How fast should it jump
        /// </summary>
        public float groundSpeed = 6.0f;

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="unit">The unit that has entered the portal.</param>
        /// <param name="from">The portal cell that was entered.</param>
        /// <param name="to">The destination at the other side of the portal.</param>
        /// <param name="callWhenComplete">The callback to call when the move is complete.</param>
        public void Execute(Transform unit, PortalCell from, IPositioned to, Action callWhenComplete)
        {
            var j = new Jumper(this, unit, from, to, callWhenComplete);

            LoadBalancer.defaultBalancer.Add(j, 0f);
        }

        /// <summary>
        /// Gets the action cost.
        /// </summary>
        /// <param name="from">The node from which the action will start.</param>
        /// <param name="to">The node at which the action will end.</param>
        /// <param name="costProvider">The cost provider in use by the path finder.</param>
        /// <returns>
        /// The cost
        /// </returns>
        public int GetActionCost(IPositioned from, IPositioned to, IMoveCost costProvider)
        {
            //This is kind of an arbitrary thing. Under normal circumstances the cost of moving from one cell to another is equal to the distance between the cells.
            //Based on that the cost here would be the length of the arc of the jump. However speed is also a factor. One could get a reference to the IDefine speed component and use the current speed setting,
            //but we want to use a different speed for jumps, so to be accurate we would need to determine the speed difference and take that into account.
            //We do not want this calculation to be overly complex however, so for this example we keep it simple by using assumptions. This will produce a less accurate cost but...
            var halfDistance = (to.position - from.position).magnitude / 2.0f;
            var hmax = this.heightFactor * halfDistance * 2.0f;
            var assumedNormalSpeed = 3.0f;

            return (int)(2 * Mathf.Sqrt((halfDistance * halfDistance) + (hmax * hmax)) / (this.groundSpeed / assumedNormalSpeed)) * costProvider.baseMoveCost;
        }

        private class Jumper : ILoadBalanced
        {
            private Transform _unit;
            private Vector3 _direction;
            private float _distance;
            private float _hmax;
            private float _alpha;
            private float _beta;
            private float _charlie;
            private float _speed;
            private Quaternion _targetRotation;
            private Action _callBack;
            private float _distanceTravelled;

            public Jumper(PortalActionJumpComponent issuer, Transform unit, PortalCell from, IPositioned to, Action callWhenComplete)
            {
                _unit = unit;
                _callBack = callWhenComplete;

                var dir = to.position - unit.position.AdjustAxis(0.0f, Axis.Y);
                _distance = dir.magnitude;
                _direction = dir / _distance;

                float fromHeight;
                if (!GameServices.heightStrategy.heightSampler.TrySampleHeight(_unit.position, out fromHeight))
                {
                    //If we aren't height sampling, just assume that from height is identical to destination height
                    fromHeight = to.position.y;
                }

                _hmax = issuer.heightFactor * _distance;
                _alpha = Mathf.PI / _distance;
                _beta = (to.position.y - fromHeight) / _distance;
                _charlie = _unit.position.y;

                _speed = issuer.groundSpeed;

                _targetRotation = Quaternion.LookRotation(dir, Vector3.up);

                this.repeat = true;
            }

            public bool repeat
            {
                get;
                private set;
            }

            public float? ExecuteUpdate(float deltaTime, float nextInterval)
            {
                //Make the speed start out at full speed and reduce to half at the peak and pick up to full again.
                var speedAdjust = 1 - ((0.5f / _hmax) * (_unit.position.y - _charlie));
                var deltaDist = _speed * deltaTime * speedAdjust;
                _distanceTravelled += deltaDist;

                var moveVector = _direction * deltaDist;
                moveVector.y = ((Mathf.Sin(_alpha * _distanceTravelled) * _hmax) + (_beta * _distanceTravelled) + _charlie) - _unit.position.y;

                //we just manipulate the transform directly since this happens in update and not fixed update
                _unit.position = _unit.position + moveVector;
                _unit.rotation = Quaternion.RotateTowards(_unit.rotation, _targetRotation, 180 * deltaTime);

                if (_distanceTravelled >= _distance)
                {
                    this.repeat = false;
                    _callBack();
                }

                //We want to update every frame if possible
                return 0.0f;
            }
        }
    }
}
