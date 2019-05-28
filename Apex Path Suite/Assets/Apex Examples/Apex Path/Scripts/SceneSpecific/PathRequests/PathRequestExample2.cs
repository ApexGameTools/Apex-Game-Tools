#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.PathRequests
{
    using System;
    using Apex.PathFinding;
    using Apex.Services;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/PathRequests/Path Request Example 2", 1006)]
    public class PathRequestExample2 : MonoBehaviour
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

                if (result.pathCost > 200)
                {
                    Debug.Log("Too far away!");
                    return;
                }

                unit.MoveAlong(result.path);
            };

            var req = unit.CreatePathRequest(this.target.position, callback);

            GameServices.pathService.QueueRequest(req);
        }
    }
}
