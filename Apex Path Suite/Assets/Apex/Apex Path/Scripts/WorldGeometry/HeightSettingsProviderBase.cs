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
    /// Base class for height settings providers for cells.
    /// </summary>
    public abstract class HeightSettingsProviderBase : IHeightSettingsProvider
    {
        /// <summary>
        /// Assigns height settings to a portion of the matrix.
        /// </summary>
        /// <param name="matrix">The matrix to update.</param>
        /// <param name="bounds">The portion of the matrix to update.</param>
        /// <returns>An enumerator which once enumerated will do the update.</returns>
        public abstract IEnumerator AssignHeightSettings(CellMatrix matrix, MatrixBounds bounds);

        /// <summary>
        /// Gets the perpendicular offsets.
        /// </summary>
        /// <param name="matrix">The matrix to update.</param>
        /// <param name="dx">The x delta.</param>
        /// <param name="dz">The z delta.</param>
        /// <returns>An array with 3 entries, representing left side offset, no offset and right side offset.</returns>
        protected Vector3[] GetPerpendicularOffsets(CellMatrix matrix, int dx, int dz)
        {
            Vector3 ppd;
            var obstacleSensitivityRange = Mathf.Min(matrix.cellSize / 2f, matrix.obstacleSensitivityRange);

            if (dx != 0 && dz != 0)
            {
                var offSet = obstacleSensitivityRange / Consts.SquareRootTwo;
                ppd = new Vector3(offSet * -dx, 0.0f, offSet * dz);
            }
            else
            {
                ppd = new Vector3(obstacleSensitivityRange * dz, 0.0f, obstacleSensitivityRange * dx);
            }

            return new Vector3[]
            {
                Vector3.zero,
                ppd,
                ppd * -1
            };
        }
    }
}
