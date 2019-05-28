#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.SteeringAndOrientation
{
    using Apex.Steering;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/SteeringAndOrientation/Example Orientation", 1009)]
    public class ExampleOrientation : OrientationComponent
    {
        public Transform focusPoint;
        public float minProximityToFocus = 4f;

        public override void GetOrientation(SteeringInput input, OrientationOutput output)
        {
            var targetOrientation = (this.focusPoint.position - input.unit.position).OnlyXZ();

            if (targetOrientation.sqrMagnitude > this.minProximityToFocus * this.minProximityToFocus)
            {
                return;
            }

            output.desiredOrientation = targetOrientation;
            output.desiredAngularAcceleration = GetAngularAcceleration(targetOrientation.normalized, input);
        }
    }
}
