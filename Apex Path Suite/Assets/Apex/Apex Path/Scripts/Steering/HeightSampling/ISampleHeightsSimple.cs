/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A simple height sampler
    /// </summary>
    public interface ISampleHeightsSimple
    {
        /// <summary>
        /// Samples the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="matrix">The matrix in which to sample the height.</param>
        /// <returns>The height at the position</returns>
        float SampleHeight(Vector3 position, CellMatrix matrix);
    }
}
