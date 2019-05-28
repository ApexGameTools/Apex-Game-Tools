/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using UnityEngine;

    /// <summary>
    /// Interface for cell factories
    /// </summary>
    public interface ICellFactory
    {
        /// <summary>
        /// Creates a cell.
        /// </summary>
        /// <param name="parent">The parent matrix.</param>
        /// <param name="position">The position.</param>
        /// <param name="matrixPosX">The matrix position x.</param>
        /// <param name="matrixPosZ">The matrix position z.</param>
        /// <param name="blocked">if set to <c>true</c> the cell is considered permanently blocked.</param>
        /// <returns></returns>
        Cell Create(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked);
    }
}
