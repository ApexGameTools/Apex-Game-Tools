#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.PathRequests
{
    using System;
    using Apex.PathFinding;
    using Apex.Services;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/PathRequests/Path Request Example 1", 1005)]
    public class PathRequestExample1 : MonoBehaviour
    {
        public Transform target;

        private void Start()
        {
            var unit = this.GetUnitFacade();

            Action<PathResult> callback = (result) =>
            {
                //Here we treat partial completion the same as full completion, you may want to treat partials differently
                if (!(result.status == PathingStatus.Complete || result.status == PathingStatus.CompletePartial))
                {
                    return;
                }

                unit.MoveAlong(result.path);
            };

            var req = new CallbackPathRequest(callback)
            {
                from = unit.position,
                to = this.target.position,
                requesterProperties = unit,
                pathFinderOptions = unit.pathFinderOptions
            };

            GameServices.pathService.QueueRequest(req);
        }
    }
}
