#pragma warning disable 1591
namespace Apex.Examples.Misc
{
    using Apex.Steering;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A point that draws a lot of attention.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Fixation Point", 1005)]
    public class FixationPoint : OrientationComponent
    {
        /// <summary>
        /// The fixation point
        /// </summary>
        public Transform fixationPoint;

        public override void GetOrientation(SteeringInput input, OrientationOutput output)
        {
            var targetOrientation = (fixationPoint.position - this.transform.position).OnlyXZ();
            output.desiredOrientation = targetOrientation;
            output.desiredAngularAcceleration = GetAngularAcceleration(targetOrientation.normalized, input);
        }
    }
}
