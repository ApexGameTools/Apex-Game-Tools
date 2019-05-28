#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.Steering;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Extensibility/Steer to Avoid Specific Other", 1009)]
    public class SteerToAvoidSpecificOther : SteeringComponent
    {
        public GameObject other;
        public float repulsionRange = 10.0f;

        private float _repulsionSquared;

        protected override void Awake()
        {
            base.Awake();

            _repulsionSquared = this.repulsionRange * this.repulsionRange;
        }

        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (other.Equals(null))
            {
                return;
            }

            var diff = (input.unit.position - other.transform.position);
            if (diff.sqrMagnitude > _repulsionSquared)
            {
                return;
            }

            output.desiredAcceleration = diff * ((this.repulsionRange - diff.magnitude) / this.repulsionRange);
        }
    }
}
