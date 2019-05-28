/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Clearance provider for flat cells, i.e. no height differences.
    /// </summary>
    /// <seealso cref="Apex.WorldGeometry.IClearanceProvider" />
    public sealed class FlatCellClearanceProvider : IClearanceProvider
    {
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
                var c = rawMatrix[x, 0] as FlatClearanceCell;
                c.clearance = c.isPermanentlyBlocked ? 0f : halfCell;

                c = rawMatrix[x, rows - 1] as FlatClearanceCell;
                c.clearance = c.isPermanentlyBlocked ? 0f : halfCell;
            }

            yield return null;

            for (int z = 1; z < rows - 1; z++)
            {
                var c = rawMatrix[0, z] as FlatClearanceCell;
                c.clearance = c.isPermanentlyBlocked ? 0f : halfCell;

                c = rawMatrix[columns - 1, z] as FlatClearanceCell;
                c.clearance = c.isPermanentlyBlocked ? 0f : halfCell;
            }

            yield return null;

            //Do the rest. First from left to right and the reversed
            for (int x = 1; x < columns - 1; x++)
            {
                for (int z = 1; z < rows - 1; z++)
                {
                    var c = rawMatrix[x, z] as FlatClearanceCell;
                    if (c.isPermanentlyBlocked)
                    {
                        c.clearance = 0f;
                        continue;
                    }

                    var minClear = float.MaxValue;
                    for (int nx = -1; nx < 2; nx++)
                    {
                        for (int nz = -1; nz < 2; nz++)
                        {
                            var n = rawMatrix[x + nx, z + nz] as FlatClearanceCell;

                            if (n.isPermanentlyBlocked)
                            {
                                minClear = -halfCell;
                                nz = nx = 2;
                            }
                            else if (n.clearance < float.MaxValue)
                            {
                                minClear = Mathf.Min(minClear, n.clearance);
                            }
                        }
                    }

                    c.clearance = minClear + cellSize;
                }

                yield return null;
            }

            for (int x = columns - 2; x >= 1; x--)
            {
                for (int z = 1; z < rows - 1; z++)
                {
                    var c = rawMatrix[x, z] as FlatClearanceCell;
                    if (c.isPermanentlyBlocked)
                    {
                        c.clearance = 0f;
                        continue;
                    }

                    var minClear = float.MaxValue;
                    for (int nx = -1; nx < 2; nx++)
                    {
                        for (int nz = -1; nz < 2; nz++)
                        {
                            var n = rawMatrix[x + nx, z + nz] as FlatClearanceCell;

                            if (n.isPermanentlyBlocked)
                            {
                                minClear = -halfCell;
                                nz = nx = 2;
                            }
                            else if (n.clearance < float.MaxValue)
                            {
                                minClear = Mathf.Min(minClear, n.clearance);
                            }
                        }
                    }

                    c.clearance = minClear + cellSize;
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
