/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;

    /// <summary>
    /// Interface for clearance providers.
    /// </summary>
    public interface IClearanceProvider
    {
        /// <summary>
        /// Sets the clearance values for all cells in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        IEnumerator SetClearance(CellMatrix matrix);

        /// <summary>
        /// Resets the clearance in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        IEnumerator Reset(CellMatrix matrix);
    }
}
