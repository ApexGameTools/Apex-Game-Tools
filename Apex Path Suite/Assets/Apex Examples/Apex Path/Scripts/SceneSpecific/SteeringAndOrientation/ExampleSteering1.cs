#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.SteeringAndOrientation
{
    using Apex.Steering;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/SteeringAndOrientation/Example Steering 1", 1010)]
    public class ExampleSteering1 : SteeringComponent
    {
        public GameObject other;
        public float repulsionRange = 10.0f;

        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            if (other.Equals(null))
            {
                return;
            }

            var diff = (input.unit.position - other.transform.position);
            if (diff.sqrMagnitude > this.repulsionRange * this.repulsionRange)
            {
                return;
            }

            output.desiredAcceleration = diff.normalized * (this.repulsionRange - diff.magnitude);
        }
    }
}
