/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.Common;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Request Preprocessor that will ensure that a given destination has the proper clerance and if not will alter the request to stop at a valid spot.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Request PreProcess: Ensure Destination Clearance", 1040)]
    [ApexComponent("Game World")]
    public class EnsureDestinationClearancePreProcessorComponent : MonoBehaviour, IRequestPreProcessor
    {
        private GridEvaluatorClosestMatch _eval = new GridEvaluatorClosestMatch(50);

        [SerializeField, Tooltip("The priority of the pre processor in relation to other pre processors. Highest priority is executed first.")]
        private int _priority = 100;

        /// <summary>
        /// Gets the priority, high number means high priority.
        /// </summary>
        public int priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// Pre-process the request to alter it in some way before it is passed on to the path finder.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>true</c> if the request was processed; otherwise <c>false</c>
        /// </returns>
        public bool PreProcess(IPathRequest request)
        {
            var grid = GridManager.instance.GetGrid(request.to);
            if (grid == null)
            {
                return false;
            }

            var goal = grid.GetCell(request.to, false) as IHaveClearance;
            if (goal == null)
            {
                return false;
            }

            var unitRadius = request.requesterProperties.radius;
            if (goal.clearance >= unitRadius)
            {
                return false;
            }

            var data = new MatchData
            {
                unitRadius = unitRadius,
                baseClearance = goal.clearance
            };

            var newGoal = _eval.ClosestMatch(grid, request.to, data.HasProperClearance, data.IsBlocked);
            if (newGoal != null)
            {
                request.to = newGoal.position;
                return true;
            }

            return false;
        }

        private struct MatchData
        {
            internal float unitRadius;
            internal float baseClearance;

            internal bool HasProperClearance(Cell c)
            {
                var cc = (IHaveClearance)c;
                return cc.clearance >= unitRadius;
            }

            internal bool IsBlocked(Cell c)
            {
                var cc = (IHaveClearance)c;
                return cc.clearance < baseClearance;
            }
        }
    }
}
