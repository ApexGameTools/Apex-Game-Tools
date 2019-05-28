/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using UnityEngine;

    /// <summary>
    /// Interface for collider detectors that implement a custom method of detecting whether a position is considered blocked, i.e. invalid for navigation.
    /// </summary>
    public interface IBlockDetector
    {
        /// <summary>
        /// Determines whether the specified position is blocked, i.e. invalid for navigation.
        /// </summary>
        /// <param name="matrix">The matrix for which this check is made.</param>
        /// <param name="position">The position to check.</param>
        /// <param name="blockThreshold">The block threshold, meaning that if an obstacle is detected inside this radius of the position, the position is to be considered blocked.</param>
        /// <returns><c>true</c> if the position is blocked; otherwise <c>false</c></returns>
        bool IsBlocked(CellMatrix matrix, Vector3 position, float blockThreshold);
    }
}
