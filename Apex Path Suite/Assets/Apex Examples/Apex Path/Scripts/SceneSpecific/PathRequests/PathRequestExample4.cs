#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.PathRequests
{
    using Apex.PathFinding;
    using Apex.Services;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/PathRequests/Path Request Example 4", 1008)]
    public class PathRequestExample4 : MonoBehaviour, INeedPath
    {
        public Transform target;

        private void Start()
        {
            var unit = this.GetUnitFacade();

            //The radius should of course be taken off the unit type that is doing the pick-up, but keeping it simple...
            var req = new BasicPathRequest()
            {
                from = unit.position,
                to = this.target.position,
                requester = this,
                requesterProperties = unit,
                pathFinderOptions = unit.pathFinderOptions
            };

            GameServices.pathService.QueueRequest(req);
        }

        public void ConsumePathResult(PathResult result)
        {
            var unit = this.GetUnitFacade();

            unit.MoveAlong(result.path);
        }
    }
}
