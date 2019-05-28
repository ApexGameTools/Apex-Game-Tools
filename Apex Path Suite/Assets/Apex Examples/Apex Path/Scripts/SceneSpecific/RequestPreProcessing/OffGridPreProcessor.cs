/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RequestPreProcessing
{
    using Apex.PathFinding;
    using Apex.WorldGeometry;

    /// <summary>
    /// This example pre processor ensures that if the unit is given an off-grid destination it moves to the nearest grid cell.
    /// A related post processor then appends the last leg to the path.
    /// </summary>
    public class OffGridPreProcessor : RequestPreProcessorBase
    {
        public override bool PreProcess(IPathRequest request)
        {
            var toGrid = GridManager.instance.GetGrid(request.to);
            var fromGrid = GridManager.instance.GetGrid(request.from);

            if ((fromGrid != null && toGrid != null) || (fromGrid == null && toGrid == null))
            {
                return false;
            }

            if (toGrid == null)
            {
                var goal = fromGrid.GetCell(request.to, true);
                request.customData = new PathAddition
                {
                    prepend = false,
                    point = request.to
                };

                request.to = goal.position;
            }
            else
            {
                var start = toGrid.GetCell(request.from, true);
                request.customData = new PathAddition
                {
                    prepend = true,
                    point = request.from
                };

                request.from = start.position;
            }

            return true;
        }
    }
}
