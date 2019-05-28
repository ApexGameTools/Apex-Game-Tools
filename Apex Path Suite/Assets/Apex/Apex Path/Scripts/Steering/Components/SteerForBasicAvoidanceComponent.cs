/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using Apex.Steering;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Basic steering component that ensures collision free movement of units.
    /// </summary>
    [AddComponentMenu("")]
    [ApexComponent("Steering")]
    public class SteerForBasicAvoidanceComponent : SteeringComponent
    {
        private BasicScanner _scanner;
        private float _minSpeedSquared;
        private float _fovReverseAngleCos;
        private float _omniAwareRadius;

        /// <summary>
        /// Called on Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            var unit = this.GetUnitFacade();
            _scanner = GetComponent<BasicScanner>();
            _fovReverseAngleCos = Mathf.Cos(((360f - unit.fieldOfView) / 2f) * Mathf.Deg2Rad);
            _omniAwareRadius = unit.radius * 2f;

            _minSpeedSquared = unit.minimumSpeed * unit.minimumSpeed;

            var goLayer = 1 << this.gameObject.layer;
            if ((goLayer & Layers.units) == 0)
            {
                Debug.LogWarning(this.gameObject.name + " : For local avoidance to work, your unit must be put in the unit layer(s) mapped on the Game World.");
            }
        }

        /// <summary>
        /// Gets the desired steering output.
        /// </summary>
        /// <param name="input">The steering input containing relevant information to use when calculating the steering output.</param>
        /// <param name="output">The steering output to be populated.</param>
        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (input.currentPlanarVelocity.sqrMagnitude < _minSpeedSquared)
            {
                return;
            }

            var otherUnits = _scanner.Units;
            float maxAdj = 0f;
            float adjSum = 0f;
            Vector3 moveVector = Vector3.zero;

            for (int i = 0; i < otherUnits.Length; i++)
            {
                var other = otherUnits[i];
                if (other.Equals(null) || other.Equals(input.unit.collider))
                {
                    continue;
                }

                Vector3 evadePos;
                var otherVelo = other.GetUnitFacade();

                if (otherVelo == null)
                {
                    evadePos = other.transform.position;
                }
                else
                {
                    var otherPos = otherVelo.position;
                    var distToOther = (otherPos - input.unit.position).magnitude;
                    var otherSpeed = otherVelo.velocity.magnitude;

                    var predictionTime = 0.1f;
                    if (otherSpeed > 0f)
                    {
                        //Half the prediction time for better behavior
                        predictionTime = (distToOther / otherSpeed) / 2f;
                    }

                    evadePos = otherPos + (otherVelo.velocity * predictionTime);
                }

                var offset = input.unit.position - evadePos;
                var offsetMagnitude = offset.magnitude;

                //Only do avoidance if inside vision cone or very close to the unit
                if (offsetMagnitude > _omniAwareRadius && Vector3.Dot(input.unit.forward, offset / offsetMagnitude) > _fovReverseAngleCos)
                {
                    continue;
                }

                //The adjustment normalizes the offset and adjusts its impact according to the offset length, i.e. the further the other unit is away the less it will impact the steering
                var adj = 1f / (offsetMagnitude * offsetMagnitude);
                adjSum += adj;
                if (adj > maxAdj)
                {
                    maxAdj = adj;
                }

                moveVector += (offset * adj);
            }

            if (maxAdj > 0f)
            {
                //Lastly we average out the move vector based on adjustments
                moveVector = moveVector / (adjSum / maxAdj);
                output.desiredAcceleration = Seek(input.unit.position + moveVector, input);
            }
        }
    }
}