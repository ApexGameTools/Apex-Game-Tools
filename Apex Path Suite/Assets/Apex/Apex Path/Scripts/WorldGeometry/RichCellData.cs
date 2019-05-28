/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Represents the data for a <see cref="RichCell"/>
    /// </summary>
    public sealed class RichCellData : CellMatrixData
    {
        [HideInInspector]
        [SerializeField]
        private RichCell.HeightData[] _heightData;

        [HideInInspector]
        [SerializeField]
        private int[] _heightDataIndices;

        private int _recordedEntries;

        /// <summary>
        /// Prepares for initialization.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        protected override void PrepareForInitialization(CellMatrix matrix)
        {
            _heightData = new RichCell.HeightData[matrix.columns * matrix.rows * 4];
            _heightDataIndices = new int[matrix.columns * matrix.rows];
            _recordedEntries = 0;
        }

        /// <summary>
        /// Records the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected override void RecordCellData(Cell c, int cellIdx)
        {
            //Since opposing cells share the same data, just with opposite values,
            //we only need to store half the height data of a cell. The remaining ones can be obtained from neighbours
            var cell = c as RichCell;
            var source = cell.GetHeightData();

            //Since the matrix is filled one column at a time, we store data that way too.
            StoreData(source, 0, cellIdx, 0x01);
            StoreData(source, 1, cellIdx, 0x02);
            StoreData(source, 3, cellIdx, 0x04);
            StoreData(source, 6, cellIdx, 0x08);
        }

        /// <summary>
        /// Finalizes the initialization.
        /// </summary>
        protected override void FinalizeInitialization()
        {
            //shrink the array
            var tmp = new RichCell.HeightData[_recordedEntries];
            Array.Copy(_heightData, tmp, _recordedEntries);
            _heightData = tmp;
        }

        /// <summary>
        /// Injects the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected override void InjectCellData(Cell c, int cellIdx)
        {
            var cell = c as RichCell;

            var indicesMask = _heightDataIndices[cellIdx];
            var dataIdx = indicesMask >> 4;

            //Since the matrix is filled one column at a time, we process neighbours that way too.
            dataIdx = SetData(cell, 0, 8, dataIdx, (indicesMask & 0x01) > 0);
            dataIdx = SetData(cell, 1, 7, dataIdx, (indicesMask & 0x02) > 0);
            dataIdx = SetData(cell, 3, 5, dataIdx, (indicesMask & 0x04) > 0);
            dataIdx = SetData(cell, 6, 2, dataIdx, (indicesMask & 0x08) > 0);
        }

        private void StoreData(RichCell.HeightData[] source, int sourceIdx, int cellIdx, int entryMark)
        {
            var data = source[sourceIdx];
            if (data.climbHeight > 0f || data.dropHeight > 0f || data.slope > 0f)
            {
                if (_heightDataIndices[cellIdx] == 0)
                {
                    _heightDataIndices[cellIdx] = _recordedEntries << 4;
                }

                _heightData[_recordedEntries++] = data;
                _heightDataIndices[cellIdx] |= entryMark;
            }
        }

        private int SetData(RichCell c, int selfIdx, int neighbourIdx, int dataIdx, bool dataRecorded)
        {
            RichCell.HeightData data;
            if (dataRecorded)
            {
                data = _heightData[dataIdx++];
            }
            else
            {
                data = new RichCell.HeightData();
            }

            c.SetHeightData(selfIdx, data);

            var n = GetNeighbourFromIdx(c, selfIdx);
            if (n != null)
            {
                //Neighbour values are reversed
                var neighbourData = new RichCell.HeightData
                {
                    climbHeight = data.dropHeight,
                    dropHeight = data.climbHeight,
                    slope = data.slope
                };

                n.SetHeightData(neighbourIdx, neighbourData);
            }

            return dataIdx;
        }

        private RichCell GetNeighbourFromIdx(Cell c, int selfIdx)
        {
            switch (selfIdx)
            {
                case 0:
                {
                    return c.GetNeighbour(-1, -1) as RichCell;
                }

                case 1:
                {
                    return c.GetNeighbour(0, -1) as RichCell;
                }

                case 3:
                {
                    return c.GetNeighbour(-1, 0) as RichCell;
                }

                case 6:
                {
                    return c.GetNeighbour(-1, 1) as RichCell;
                }

                default:
                {
                    throw new InvalidOperationException("A fault in the data recovery logic has occurred, this is a fatal error.");
                }
            }
        }
    }
}
