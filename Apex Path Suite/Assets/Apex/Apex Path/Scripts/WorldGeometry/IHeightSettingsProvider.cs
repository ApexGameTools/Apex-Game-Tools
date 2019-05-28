/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;
    using Apex.DataStructures;

    /// <summary>
    /// Interface for height settings providers
    /// </summary>
    public interface IHeightSettingsProvider
    {
        /// <summary>
        /// Assigns height settings to a portion of the matrix.
        /// </summary>
        /// <param name="matrix">The matrix to update.</param>
        /// <param name="bounds">The portion of the matrix to update.</param>
        /// <returns>
        /// An enumerator which once enumerated will do the update.
        /// </returns>
        IEnumerator AssignHeightSettings(CellMatrix matrix, MatrixBounds bounds);
    }
}
