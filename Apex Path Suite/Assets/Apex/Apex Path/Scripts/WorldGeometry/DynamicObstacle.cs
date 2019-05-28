/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.DataStructures;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Represents an obstacle with a dynamic nature, meaning it can be an obstacle to only some, only at certain times, etc.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Dynamic Obstacle", 1030)]
    [ApexComponent("Behaviours")]
    public partial class DynamicObstacle : DynamicObstacleBase
    {
        private IActualBounds _actualBounds;
        private IGrid _lastGrid;

        private interface IActualBounds
        {
            MatrixBounds Prepare(CellMatrix matrix, bool block);

            bool Update(Cell c);

            void Render();
        }

        partial void ExtensionInit();

        /// <summary>
        /// Called on awake.
        /// </summary>
        protected override void OnAwake()
        {
            ExtensionInit();

            if (_actualBounds == null)
            {
                _actualBounds = new AxisBounds(this.GetComponent<Collider>(), this);
            }
        }

        /// <summary>
        /// Renders the visualization of the dynamic obstacle's coverage (bounds).
        /// </summary>
        public override void RenderVisualization()
        {
            if (_actualBounds == null)
            {
                return;
            }

            _actualBounds.Render();
        }

        /// <summary>
        /// Updates the cells.
        /// </summary>
        /// <param name="block">if set to <c>true</c> blocked cells will be calculated; otherwise only unblocking will occur.</param>
        protected override void UpdateCells(bool block)
        {
            if (!block)
            {
                if (_lastGrid != null)
                {
                    UpdateCells(_lastGrid, false);
                    _lastGrid = null;
                }

                return;
            }

            var grid = GridManager.instance.GetGrid(_transform.position);

            if (grid != _lastGrid && _lastGrid != null)
            {
                UpdateCells(_lastGrid, false);
            }

            _lastGrid = grid;

            if (grid != null)
            {
                UpdateCells(grid, block);
            }
        }

        private void UpdateCells(IGrid grid, bool block)
        {
            var matrix = grid.cellMatrix;

            //Create the combined matrix bounds, covering both those to unblock and block
            var combinedCoverage = _actualBounds.Prepare(matrix, block);

            var rawMatrix = matrix.rawMatrix;

            //Unblock those that are no longer blocked, and block those not yet blocked
            bool changed = false;
            for (int x = combinedCoverage.minColumn; x <= combinedCoverage.maxColumn; x++)
            {
                for (int z = combinedCoverage.minRow; z <= combinedCoverage.maxRow; z++)
                {
                    var c = rawMatrix[x, z];

                    changed |= _actualBounds.Update(c);
                }
            }

            if (changed && this.causesReplanning)
            {
                grid.TouchSections(combinedCoverage);
            }
        }

        private sealed class AxisBounds : IActualBounds
        {
            private readonly Collider _collider;
            private readonly DynamicObstacle _parent;
            private MatrixBounds _lastCoverage;
            private MatrixBounds _newCoverage;

            public AxisBounds(Collider collider, DynamicObstacle parent)
            {
                _collider = collider;
                _parent = parent;
                _lastCoverage = _newCoverage = MatrixBounds.nullBounds;
            }

            public MatrixBounds Prepare(CellMatrix matrix, bool block)
            {
                _lastCoverage = _newCoverage;

                if (!block)
                {
                    _newCoverage = MatrixBounds.nullBounds;
                    return _lastCoverage;
                }

                var velocity = _parent.GetVelocity();

                var sensitivity = (matrix.cellSize / 2f) - (_parent.useGridObstacleSensitivity ? matrix.obstacleSensitivityRange : _parent.customSensitivity);

                var bounds = GrowBoundsByVelocity(_collider.bounds, velocity);

                _newCoverage = matrix.GetMatrixBounds(bounds, sensitivity, true);

                return MatrixBounds.Combine(_lastCoverage, _newCoverage);
            }

            public bool Update(Cell c)
            {
                var x = c.matrixPosX;
                var z = c.matrixPosZ;

                if (_lastCoverage.Contains(x, z) && !_newCoverage.Contains(x, z))
                {
                    return c.RemoveDynamicObstacle(_parent);
                }
                else if (!_lastCoverage.Contains(x, z) && _newCoverage.Contains(x, z))
                {
                    return c.AddDynamicObstacle(_parent);
                }

                return false;
            }

            public void Render()
            {
                /* No real reason to support this, its pretty obvious without visual debugging */
            }

            private static Bounds GrowBoundsByVelocity(Bounds bounds, Vector3 velocity)
            {
                if (velocity.x != 0f || velocity.z != 0f)
                {
                    var vMin = bounds.min;
                    var vMax = bounds.max;

                    if (velocity.x < 0f)
                    {
                        vMin.x += velocity.x;
                    }
                    else if (velocity.x > 0f)
                    {
                        vMax.x += velocity.x;
                    }

                    if (velocity.z < 0f)
                    {
                        vMin.z += velocity.z;
                    }
                    else if (velocity.z > 0f)
                    {
                        vMax.z += velocity.z;
                    }

                    bounds.SetMinMax(vMin, vMax);
                }

                return bounds;
            }
        }
    }
}
