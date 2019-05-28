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
    /// The height settings provider for <see cref="StandardCell"/>s
    /// </summary>
    public sealed class StandardCellHeightSettingsProvider : HeightSettingsProviderBase
    {
        private float _maxWalkableSlopeAngle;
        private float _maxClimbHeight;
        private float _maxDropHeight;
        private float _granularity;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardCellHeightSettingsProvider"/> class.
        /// </summary>
        public StandardCellHeightSettingsProvider()
        {
            var settings = GameServices.heightStrategy.unitsHeightNavigationCapability;
            _maxWalkableSlopeAngle = settings.maxSlopeAngle;
            _maxClimbHeight = settings.maxClimbHeight;
            _maxDropHeight = settings.maxDropHeight;
            _granularity = GameServices.heightStrategy.sampleGranularity;
        }

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
            var inner = new InnerProvider(matrix, this);
            return inner.AssignHeightSettings(bounds);
        }

        private class InnerProvider
        {
            private float _maxWalkableSlopeAngle;
            private float _maxClimbHeight;
            private float _maxDropHeight;
            private float _granularity;
            private CellMatrix _matrix;
            private ISampleHeightsSimple _heightSampler;
            private Vector3[] _offsets;

            public InnerProvider(CellMatrix matrix, StandardCellHeightSettingsProvider parent)
            {
                _matrix = matrix;
                _maxWalkableSlopeAngle = parent._maxWalkableSlopeAngle;
                _maxClimbHeight = parent._maxClimbHeight;
                _maxDropHeight = parent._maxDropHeight;
                _granularity = parent._granularity;

                _offsets = new Vector3[3];
                _heightSampler = (GameServices.heightStrategy.heightMode == HeightSamplingMode.HeightMap) ? (ISampleHeightsSimple)matrix : GameServices.heightStrategy.heightSampler;
            }

            public IEnumerator AssignHeightSettings(MatrixBounds bounds)
            {
                if (_maxWalkableSlopeAngle >= 90f)
                {
                    yield break;
                }

                var ledgeThreshold = Mathf.Tan(GameServices.heightStrategy.ledgeThreshold * Mathf.Deg2Rad) * _matrix.granularity;
                var maxHeightDiff = Mathf.Tan(_maxWalkableSlopeAngle * Mathf.Deg2Rad) * _granularity;
                var maxHeightDiffDiag = maxHeightDiff * Consts.SquareRootTwo;

                var maxColumn = bounds.maxColumn;
                var maxRow = bounds.maxRow;
                bool isPartial = (maxColumn - bounds.minColumn < _matrix.columns - 1) || (maxRow - bounds.minRow < _matrix.rows - 1);

                var raw_matrix = _matrix.rawMatrix;
                for (int x = bounds.minColumn; x <= maxColumn; x++)
                {
                    for (int z = bounds.minRow; z <= maxRow; z++)
                    {
                        var c = raw_matrix[x, z] as StandardCell;
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

                                var n = _matrix[x + dx, z + dz] as StandardCell;
                                if (n == null || n.isPermanentlyBlocked)
                                {
                                    continue;
                                }

                                var pos = n.GetRelativePositionTo(c);
                                if (!isPartial && (c.heightIntializedFrom & pos) > 0)
                                {
                                    continue;
                                }

                                //If doing a partial update we need to remove the blocked flag from the neighbour first since he may not be processed on his own.
                                if (isPartial)
                                {
                                    c.heightBlockedFrom &= ~pos;
                                    n.heightBlockedFrom &= ~c.GetRelativePositionTo(n);
                                }

                                switch (pos)
                                {
                                    //Straight neighbours
                                    case NeighbourPosition.Bottom:
                                    case NeighbourPosition.Top:
                                    case NeighbourPosition.Left:
                                    case NeighbourPosition.Right:
                                    {
                                        UpdateCellHeightData(c, n, pos, maxHeightDiff, ledgeThreshold, true);
                                        break;
                                    }

                                    //diagonals
                                    default:
                                    {
                                        UpdateCellHeightData(c, n, pos, maxHeightDiffDiag, ledgeThreshold, false);
                                        break;
                                    }
                                }

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
                var bll = _matrix[bounds.minColumn - 1, bounds.minRow] as StandardCell;
                var blb = _matrix[bounds.minColumn, bounds.minRow - 1] as StandardCell;

                var tll = _matrix[bounds.minColumn - 1, bounds.maxRow] as StandardCell;
                var tlt = _matrix[bounds.minColumn, bounds.maxRow + 1] as StandardCell;

                var brr = _matrix[bounds.maxColumn + 1, bounds.minRow] as StandardCell;
                var brb = _matrix[bounds.maxColumn, bounds.minRow - 1] as StandardCell;

                var trr = _matrix[bounds.maxColumn + 1, bounds.maxRow] as StandardCell;
                var trt = _matrix[bounds.maxColumn, bounds.maxRow + 1] as StandardCell;

                if (bll != null && blb != null)
                {
                    bll.heightBlockedFrom &= ~NeighbourPosition.BottomRight;
                    blb.heightBlockedFrom &= ~NeighbourPosition.TopLeft;
                    UpdateCellHeightData(bll, blb, NeighbourPosition.BottomRight, maxHeightDiffDiag, ledgeThreshold, false);
                    yield return null;
                }

                if (tll != null && tlt != null)
                {
                    tll.heightBlockedFrom &= ~NeighbourPosition.TopRight;
                    tlt.heightBlockedFrom &= ~NeighbourPosition.BottomLeft;
                    UpdateCellHeightData(tll, tlt, NeighbourPosition.TopRight, maxHeightDiffDiag, ledgeThreshold, false);
                    yield return null;
                }

                if (brr != null && brb != null)
                {
                    brr.heightBlockedFrom &= ~NeighbourPosition.BottomLeft;
                    brb.heightBlockedFrom &= ~NeighbourPosition.TopRight;
                    UpdateCellHeightData(brr, brb, NeighbourPosition.BottomLeft, maxHeightDiffDiag, ledgeThreshold, false);
                    yield return null;
                }

                if (trr != null && trt != null)
                {
                    trr.heightBlockedFrom &= ~NeighbourPosition.TopLeft;
                    trt.heightBlockedFrom &= ~NeighbourPosition.BottomRight;
                    UpdateCellHeightData(trr, trt, NeighbourPosition.TopLeft, maxHeightDiffDiag, ledgeThreshold, false);
                    yield return null;
                }
            }

            private void UpdateCellHeightData(StandardCell reference, StandardCell neighbour, NeighbourPosition neighbourPos, float maxHeightDiff, float ledgeThreshold, bool testForPermanentBlock)
            {
                var dir = reference.GetDirectionTo(neighbour);
                GetPerpendicularOffsets(dir.x, dir.z);

                var granularity = _matrix.granularity;
                var steps = _matrix.cellSize / granularity;

                var maxClimb = 0f;
                var maxDrop = 0f;

                for (int o = 0; o < 3; o++)
                {
                    var samplePos = reference.position + _offsets[o];
                    var fromHeight = _heightSampler.SampleHeight(samplePos, _matrix);

                    var climbAccumulator = 0.0f;
                    var dropAccumulator = 0.0f;

                    for (int i = 0; i < steps; i++)
                    {
                        samplePos.x += (dir.x * granularity);
                        samplePos.z += (dir.z * granularity);

                        var toHeight = _heightSampler.SampleHeight(samplePos, _matrix);

                        var heightDiff = toHeight - fromHeight;
                        var absDiff = Mathf.Abs(heightDiff);

                        bool ledgeEncountered = absDiff < ledgeThreshold;

                        if (absDiff <= maxHeightDiff || ledgeEncountered)
                        {
                            if (climbAccumulator > maxClimb)
                            {
                                maxClimb = climbAccumulator;
                            }
                            else if (dropAccumulator > maxDrop)
                            {
                                maxDrop = dropAccumulator;
                            }

                            climbAccumulator = 0f;
                            dropAccumulator = 0f;
                        }
                        else if (heightDiff > 0f)
                        {
                            climbAccumulator += heightDiff;
                            if (dropAccumulator > maxDrop)
                            {
                                maxDrop = dropAccumulator;
                            }

                            dropAccumulator = 0f;
                        }
                        else if (heightDiff < 0f)
                        {
                            dropAccumulator += absDiff;
                            if (climbAccumulator > maxClimb)
                            {
                                maxClimb = climbAccumulator;
                            }

                            climbAccumulator = 0f;
                        }

                        fromHeight = toHeight;
                    } /* end steps */

                    maxClimb = Mathf.Max(climbAccumulator, maxClimb);
                    maxDrop = Mathf.Max(dropAccumulator, maxDrop);
                }

                var refPos = reference.GetRelativePositionTo(neighbour);
                if (maxClimb > _maxClimbHeight || maxDrop > _maxDropHeight)
                {
                    neighbour.heightBlockedFrom |= refPos;
                }

                if (maxClimb > _maxDropHeight || maxDrop > _maxClimbHeight)
                {
                    reference.heightBlockedFrom |= neighbourPos;
                }

                reference.heightIntializedFrom |= neighbourPos;
                neighbour.heightIntializedFrom |= refPos;
            } /* end method */

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
