/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;
    using Apex.DataStructures;
    using Apex.Services;
    using Apex.Steering;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// The height settings provider for <see cref="RichCell"/>s
    /// </summary>
    public sealed class RichCellHeightSettingsProvider : HeightSettingsProviderBase
    {
        /// <summary>
        /// Assigns height settings to a portion of the _matrix.
        /// </summary>
        /// <param name="matrix">The matrix to update.</param>
        /// <param name="bounds">The portion of the matrix to update.</param>
        /// <returns>
        /// An enumerator which once enumerated will do the update.
        /// </returns>
        public override IEnumerator AssignHeightSettings(CellMatrix matrix, MatrixBounds bounds)
        {
            var inner = new InnerProvider(matrix);
            return inner.AssignHeightSettings(bounds);
        }

        private class InnerProvider
        {
            private CellMatrix _matrix;
            private ISampleHeightsSimple _heightSampler;
            private Vector3[] _offsets;

            public InnerProvider(CellMatrix matrix)
            {
                _matrix = matrix;
                _offsets = new Vector3[3];
                _heightSampler = (GameServices.heightStrategy.heightMode == HeightSamplingMode.HeightMap) ? (ISampleHeightsSimple)_matrix : GameServices.heightStrategy.heightSampler;
            }

            public IEnumerator AssignHeightSettings(MatrixBounds bounds)
            {
                var maxColumn = bounds.maxColumn;
                var maxRow = bounds.maxRow;
                bool isPartial = (maxColumn - bounds.minColumn < _matrix.columns - 1) || (maxRow - bounds.minRow < _matrix.rows - 1);
                var ledgeThreshold = Mathf.Tan(GameServices.heightStrategy.ledgeThreshold * Mathf.Deg2Rad) * _matrix.granularity;

                var rawMatrix = _matrix.rawMatrix;
                for (int x = bounds.minColumn; x <= maxColumn; x++)
                {
                    for (int z = bounds.minRow; z <= maxRow; z++)
                    {
                        var c = rawMatrix[x, z] as RichCell;
                        if (c.isPermanentlyBlocked)
                        {
                            continue;
                        }

                        //Process neighbours
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dz == 0)
                                {
                                    continue;
                                }

                                var n = _matrix[x + dx, z + dz] as RichCell;
                                if (n == null || n.isPermanentlyBlocked)
                                {
                                    continue;
                                }

                                //If height data has already been assigned (by neighbour) we don't want to do it again,
                                //unless its a partial in which case we have to since neighbours are not necessarily covered by the update
                                if (!isPartial && c.HasInitializedHeightData(dx, dz))
                                {
                                    continue;
                                }

                                UpdateCellHeightData(c, n, ledgeThreshold, dx, dz);

                                yield return null;
                            }
                        } /* end neighbour loop */
                    }
                }

                if (!isPartial)
                {
                    yield break;
                }

                //Update corner diagonals, this is only relevant for partial updates
                //Since the cells being updated only update their own relation to neighbours, there will potentially be 4 connections not updated,
                //those are the diagonals between the cells surround each corner of the bounds, e.g. bottom left corner the connection between the cell to the left and the cell below that corner.
                //since updates also update the involved neighbour, we only have to update 4 additional cells, and only on a specific diagonal.
                var bll = _matrix[bounds.minColumn - 1, bounds.minRow] as RichCell;
                var blb = _matrix[bounds.minColumn, bounds.minRow - 1] as RichCell;

                var tll = _matrix[bounds.minColumn - 1, bounds.maxRow] as RichCell;
                var tlt = _matrix[bounds.minColumn, bounds.maxRow + 1] as RichCell;

                var brr = _matrix[bounds.maxColumn + 1, bounds.minRow] as RichCell;
                var brb = _matrix[bounds.maxColumn, bounds.minRow - 1] as RichCell;

                var trr = _matrix[bounds.maxColumn + 1, bounds.maxRow] as RichCell;
                var trt = _matrix[bounds.maxColumn, bounds.maxRow + 1] as RichCell;

                if (bll != null && blb != null)
                {
                    UpdateCellHeightData(bll, blb, ledgeThreshold, 1, -1);
                    yield return null;
                }

                if (tll != null && tlt != null)
                {
                    UpdateCellHeightData(tll, tlt, ledgeThreshold, 1, 1);
                    yield return null;
                }

                if (brr != null && brb != null)
                {
                    UpdateCellHeightData(brr, brb, ledgeThreshold, -1, -1);
                    yield return null;
                }

                if (trr != null && trt != null)
                {
                    UpdateCellHeightData(trr, trt, ledgeThreshold, -1, 1);
                    yield return null;
                }
            }

            private void UpdateCellHeightData(RichCell reference, RichCell neighbour, float ledgeThreshold, int dx, int dz)
            {
                GetPerpendicularOffsets(dx, dz);
                var granularity = _matrix.granularity;
                var steps = _matrix.cellSize / granularity;

                var data = new RichCell.HeightData();

                for (int o = 0; o < 3; o++)
                {
                    var samplePos = reference.position + _offsets[o];
                    var fromHeight = _heightSampler.SampleHeight(samplePos, _matrix);

                    var climbAccumulator = 0.0f;
                    var dropAccumulator = 0.0f;

                    for (int i = 0; i < steps; i++)
                    {
                        samplePos.x += (dx * granularity);
                        samplePos.z += (dz * granularity);

                        var toHeight = _heightSampler.SampleHeight(samplePos, _matrix);

                        var heightDiff = toHeight - fromHeight;
                        var absDiff = Mathf.Abs(heightDiff);

                        //Initially we just record the slope as the height diff
                        if (data.slope < absDiff)
                        {
                            data.slope = absDiff;
                        }

                        if (absDiff < ledgeThreshold)
                        {
                            if (data.dropHeight < dropAccumulator)
                            {
                                data.dropHeight = dropAccumulator;
                            }
                            else if (data.climbHeight < climbAccumulator)
                            {
                                data.climbHeight = climbAccumulator;
                            }

                            dropAccumulator = 0f;
                            climbAccumulator = 0f;
                        }

                        if (heightDiff > 0f)
                        {
                            climbAccumulator += heightDiff;

                            if (data.dropHeight < dropAccumulator)
                            {
                                data.dropHeight = dropAccumulator;
                            }

                            dropAccumulator = 0f;
                        }
                        else if (heightDiff < 0f)
                        {
                            dropAccumulator += absDiff;

                            if (data.climbHeight < climbAccumulator)
                            {
                                data.climbHeight = climbAccumulator;
                            }

                            climbAccumulator = 0f;
                        }

                        fromHeight = toHeight;
                    }

                    //Make sure we get the last accumulation recorded
                    if (data.dropHeight < dropAccumulator)
                    {
                        data.dropHeight = dropAccumulator;
                    }
                    else if (data.climbHeight < climbAccumulator)
                    {
                        data.climbHeight = climbAccumulator;
                    }
                } /* end for each offset */

                //Set the slope to an angular value
                var mod = (dx != 0 && dz != 0) ? Consts.SquareRootTwo : 1f;
                data.slope = Mathf.Atan(data.slope / (granularity * mod)) * Mathf.Rad2Deg;

                //Set the data
                reference.SetHeightData(dx, dz, data);
                reference.CalculateWorst();

                //Create the neighbour data as the reverse of this, i.e. drop = climb and vice versa
                var neighbourData = new RichCell.HeightData(data.slope, data.dropHeight, data.climbHeight);
                neighbour.SetHeightData(-dx, -dz, neighbourData);
                neighbour.CalculateWorst();
            }

            private void GetPerpendicularOffsets(int dx, int dz)
            {
                Vector3 ppd;
                var obstacleSensitivityRange = Mathf.Min(_matrix.cellSize * 0.5f, _matrix.obstacleSensitivityRange);

                if (dx != 0 && dz != 0)
                {
                    var offSet = obstacleSensitivityRange / Consts.SquareRootTwo;
                    ppd = new Vector3(offSet * -dx, 0.0f, offSet * dz);
                }
                else
                {
                    ppd = new Vector3(obstacleSensitivityRange * dz, 0.0f, obstacleSensitivityRange * dx);
                }

                _offsets[1] = ppd;
                _offsets[2] = ppd * -1f;
            }
        }
    }
}
