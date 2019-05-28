#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.SteeringAndOrientation
{
    using Apex.Steering;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/SteeringAndOrientation/Example Steering 2", 1011)]
    public class ExampleSteering2 : SteeringComponent
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

            var targetPos = input.unit.position + (diff.normalized * (this.repulsionRange - diff.magnitude));

            output.desiredAcceleration = Seek(targetPos, input);
        }
    }
}
