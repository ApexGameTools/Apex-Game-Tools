/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.PathRequests
{
    using Apex.PathFinding;
    using Apex.Services;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/PathRequests/Manual Path Example", 1009)]
    public class ManualPathExample : MonoBehaviour
    {
        public Vector3[] points;

        private void Start()
        {
            var unit = this.GetUnitFacade();

            var path = new Path(points);

            //If the first point on the path is too far (further than Next Node Distance) from the unit,
            //steer for path will truncate the path such that it will skip to the path point closest to the unit which can be reached (unobstructed straight line).
            //For this reason we simply add the unit's position (actually the unit itself as it implements IPositioned) as the first point on the path. You could of course make this conditional.
            path.Push(unit);

            unit.MoveAlong(path);
        }
    }
}
