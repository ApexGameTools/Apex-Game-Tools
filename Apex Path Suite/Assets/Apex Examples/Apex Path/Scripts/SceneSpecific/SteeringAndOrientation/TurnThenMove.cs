#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.SteeringAndOrientation
{
    using Apex.PathFinding;
    using Apex.Steering;
    using UnityEngine;

    public class TurnThenMove : OrientationComponent
    {
        public override void GetOrientation(SteeringInput input, OrientationOutput output)
        {
            var unit = input.unit;

            if (!unit.nextNodePosition.HasValue)
            {
                unit.Resume();
                return;
            }

            var targetOrientation = (unit.nextNodePosition.Value - unit.position).OnlyXZ().normalized;

            if (Vector3.Dot(unit.forward, targetOrientation) < 0.985f)
            {
                unit.Wait(null);
            }
            else
            {
                unit.Resume();
            }

            output.desiredOrientation = targetOrientation;
            output.desiredAngularAcceleration = GetAngularAcceleration(targetOrientation, input);
        }
    }
}
