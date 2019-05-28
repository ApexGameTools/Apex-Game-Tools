/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Represents the data for a <see cref="StandardCell"/>
    /// </summary>
    public sealed class StandardCellData : CellMatrixData
    {
        [HideInInspector]
        [SerializeField]
        private NeighbourPosition[] _heightBlockStatus;

        /// <summary>
        /// Prepares for initialization.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        protected override void PrepareForInitialization(CellMatrix matrix)
        {
            _heightBlockStatus = new NeighbourPosition[matrix.columns * matrix.rows];
        }

        /// <summary>
        /// Records the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected override void RecordCellData(Cell c, int cellIdx)
        {
            var cell = c as StandardCell;
            _heightBlockStatus[cellIdx] = cell.heightBlockedFrom;
        }

        /// <summary>
        /// Injects the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected override void InjectCellData(Cell c, int cellIdx)
        {
            var cell = c as StandardCell;
            cell.heightBlockedFrom = _heightBlockStatus[cellIdx];
        }
    }
}
