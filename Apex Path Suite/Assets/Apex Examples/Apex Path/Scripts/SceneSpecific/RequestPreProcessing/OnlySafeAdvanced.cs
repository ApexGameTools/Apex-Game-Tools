/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RequestPreProcessing
{
    using Apex.Common;
    using Apex.PathFinding;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// This example pre processor ensures that a unit can only move to safe spots.
    /// </summary>
    public class OnlySafeAdvanced : MonoBehaviour, IRequestPreProcessor
    {
        private GridEvaluatorClosestMatch _eval = new GridEvaluatorClosestMatch(50);
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

            if (SafeHavens.IsSafe(request.to))
            {
                return false;
            }

            var data = new EvalData(request.requesterProperties);
            var best = _eval.ClosestMatch(grid, request.to, data.IsItSafe, data.IsItBlocked);
            if (best == null)
            {
                return false;
            }

            request.to = best.position;
            return true;
        }

        private void OnEnable()
        {
            _enabled = true;
        }

        private void OnDisable()
        {
            _enabled = false;
        }

        private struct EvalData
        {
            private IUnitProperties _unit;

            public EvalData(IUnitProperties unit)
            {
                _unit = unit;
            }

            public bool IsItSafe(Cell c)
            {
                return SafeHavens.IsSafe(c.position);
            }

            public bool IsItBlocked(Cell c)
            {
                return !c.IsWalkableWithClearance(_unit);
            }
        }
    }
}
