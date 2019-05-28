/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RequestPreProcessing
{
    using Apex.Common;
    using Apex.PathFinding;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// This example pre processor ensures that a unit is moved to the nearest even index cell and always move to the center.
    /// It is of course completely useless besides showing how a preprocessor works.
    /// </summary>
    public class OnlyEvenAdvanced : MonoBehaviour, IRequestPreProcessor
    {
        private GridEvaluatorClosestMatch _eval = new GridEvaluatorClosestMatch(8);
        private bool _enabled;

        [SerializeField]
        private int _priority = 2;

        public int priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public bool PreProcess(IPathRequest request)
        {
            if (!_enabled)
            {
                return false;
            }

            var grid = GridManager.instance.GetGrid(request.to);
            if (grid == null)
            {
                return false;
            }

            var goal = grid.GetCell(request.to, true);
            if (IsEvenIndex(goal))
            {
                request.to = goal.position;
                return true;
            }

            var best = _eval.ClosestMatch(grid, request.to, IsEvenIndex, Discard);
            request.to = best.position;
            return true;
        }

        private bool IsEvenIndex(Cell c)
        {
            return c.matrixPosX % 2 == 0 && c.matrixPosZ % 2 == 0;
        }

        private bool Discard(Cell c)
        {
            /* Nothing to do here, since we will always find a match within the first set of neighbours.
             * However the purpose of this is to stop expansion in a neighbours direction, e.g. it is blocked */
            return false;
        }

        private void OnEnable()
        {
            _enabled = true;
        }

        private void OnDisable()
        {
            _enabled = false;
        }
    }
}
