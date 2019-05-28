#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.PathRequests
{
    using System;
    using Apex.PathFinding;
    using Apex.Services;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/PathRequests/Path Request Example 3", 1007)]
    public class PathRequestExample3 : MonoBehaviour
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

                Debug.Log("Cost of path was: " + result.pathCost);
            };

            var req = unit.CreatePathRequest(this.target.position, callback);

            req.type = RequestType.IntelOnly;

            GameServices.pathService.QueueRequest(req);
        }
    }
}
