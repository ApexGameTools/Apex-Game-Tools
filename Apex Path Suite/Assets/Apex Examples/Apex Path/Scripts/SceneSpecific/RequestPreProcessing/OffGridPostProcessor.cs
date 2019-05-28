/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RequestPreProcessing
{
    using Apex.PathFinding;
    using Apex.Steering.Components;

    /// <summary>
    /// This example post appends to the path as dictated by an <see cref="OffGridPreProcessor"/> .
    /// </summary>
    public class OffGridPostProcessor : SteerForPathResultProcessorComponent
    {
        public override bool HandleResult(PathResult result, SteerForPathComponent steerer)
        {
            if (result.status != PathingStatus.Complete || !(result.originalRequest.customData is PathAddition))
            {
                return false;
            }

            var pathAdd = (PathAddition)result.originalRequest.customData;
            if (pathAdd.prepend)
            {
                result.path.Push(pathAdd.point.AsPositioned());
            }
            else
            {
                result.path.Append(pathAdd.point.AsPositioned());
            }

            return true;
        }
    }
}
