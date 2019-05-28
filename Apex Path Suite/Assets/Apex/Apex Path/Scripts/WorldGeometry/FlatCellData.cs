/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Represents the data for a <see cref="FlatCell"/>
    /// </summary>
    public sealed class FlatCellData : CellMatrixData
    {
        /// <summary>
        /// Prepares for initialization.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        protected override void PrepareForInitialization(CellMatrix matrix)
        {
            /* NOOP */
        }

        /// <summary>
        /// Records the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected override void RecordCellData(Cell c, int cellIdx)
        {
            /* NOOP */
        }

        /// <summary>
        /// Injects the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected override void InjectCellData(Cell c, int cellIdx)
        {
            /* NOOP */
        }
    }
}
