/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;
    using Apex.Common;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Clearance provider used for both <see cref="StandardCell"/>s and <see cref="RichCell"/>s.
    /// </summary>
    public sealed class StandardCellClearanceProvider : IClearanceProvider
    {
        private float _heightDiffThreshold = 1f;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardCellClearanceProvider"/> class.
        /// </summary>
        /// <param name="heightDiffThreshold">The height difference threshold.</param>
        public StandardCellClearanceProvider(float heightDiffThreshold)
        {
            _heightDiffThreshold = heightDiffThreshold;
        }

        /// <summary>
        /// Sets the clearance values for all cells in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns></returns>
        public IEnumerator SetClearance(CellMatrix matrix)
        {
            var rows = matrix.rows;
            var columns = matrix.columns;
            var cellSize = matrix.cellSize;
            var halfCell = cellSize * 0.5f;
            var rawMatrix = matrix.rawMatrix;

            //First set the perimeter which is always 0 or ½ cell size. This allows us to use the raw matrix for the rest, skipping index range checks.
            for (int x = 0; x < columns; x++)
            {
                var c = rawMatrix[x, 0];
                var cc = c as IHaveClearance;
                cc.clearance = c.isPermanentlyBlocked ? 0f : halfCell;

                c = rawMatrix[x, rows - 1];
                cc = c as IHaveClearance;
                cc.clearance = c.isPermanentlyBlocked ? 0f : halfCell;
            }

            yield return null;

            for (int z = 1; z < rows - 1; z++)
            {
                var c = rawMatrix[0, z];
                var cc = c as IHaveClearance;
                cc.clearance = c.isPermanentlyBlocked ? 0f : halfCell;

                c = rawMatrix[columns - 1, z];
                cc = c as IHaveClearance;
                cc.clearance = c.isPermanentlyBlocked ? 0f : halfCell;
            }

            yield return null;

            //Do the rest. First from left to right and the reversed
            for (int x = 1; x < columns - 1; x++)
            {
                for (int z = 1; z < rows - 1; z++)
                {
                    var c = rawMatrix[x, z];
                    var cc = c as IHaveClearance;
                    if (c.isPermanentlyBlocked)
                    {
                        cc.clearance = 0f;
                        continue;
                    }

                    var cy = c.position.y;
                    var minClear = float.MaxValue;
                    for (int nx = -1; nx < 2; nx++)
                    {
                        for (int nz = -1; nz < 2; nz++)
                        {
                            var n = rawMatrix[x + nx, z + nz];
                            var nc = n as IHaveClearance;

                            //Height is only blocking if the neighbour is higher. Lower neighbours do not affect clearance, e..g the unit is allowed to hover partly in open air.
                            if (n.isPermanentlyBlocked || (n.position.y - cy) > _heightDiffThreshold)
                            {
                                minClear = -halfCell;
                                nz = nx = 2;
                            }
                            else if (nc.clearance < float.MaxValue)
                            {
                                minClear = Mathf.Min(minClear, nc.clearance);
                            }
                        }
                    }

                    cc.clearance = minClear + cellSize;
                }

                yield return null;
            }

            for (int x = columns - 2; x >= 1; x--)
            {
                for (int z = 1; z < rows - 1; z++)
                {
                    var c = rawMatrix[x, z];
                    var cc = c as IHaveClearance;
                    if (c.isPermanentlyBlocked)
                    {
                        cc.clearance = 0f;
                        continue;
                    }

                    var cy = c.position.y;
                    var minClear = float.MaxValue;
                    for (int nx = -1; nx < 2; nx++)
                    {
                        for (int nz = -1; nz < 2; nz++)
                        {
                            var n = rawMatrix[x + nx, z + nz];
                            var nc = n as IHaveClearance;

                            if (n.isPermanentlyBlocked || (n.position.y - cy) > _heightDiffThreshold)
                            {
                                minClear = -halfCell;
                                nz = nx = 2;
                            }
                            else if (nc.clearance < float.MaxValue)
                            {
                                minClear = Mathf.Min(minClear, nc.clearance);
                            }
                        }
                    }

                    cc.clearance = minClear + cellSize;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Resets the clearance in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns></returns>
        public IEnumerator Reset(CellMatrix matrix)
        {
            var rows = matrix.rows;
            var columns = matrix.columns;
            var rawMatrix = matrix.rawMatrix;

            for (int x = 1; x < columns - 1; x++)
            {
                for (int z = 1; z < rows - 1; z++)
                {
                    var cc = rawMatrix[x, z] as IHaveClearance;
                    cc.clearance = float.MaxValue;
                }

                yield return null;
            }
        }
    }
}
