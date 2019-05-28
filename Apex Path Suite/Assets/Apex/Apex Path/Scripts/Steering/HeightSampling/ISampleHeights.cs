/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Interface for height samplers
    /// </summary>
    public interface ISampleHeights : ISampleHeightsSimple
    {
        /// <summary>
        /// Gets the sampling strategy for the sampler.
        /// </summary>
        /// <value>
        /// The sampling strategy.
        /// </value>
        HeightSamplingMode samplingStrategy { get; }

        /// <summary>
        /// Samples the height at the specified position. . The matrix is resolved from the position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The height at the position</returns>
        float SampleHeight(Vector3 position);

        /// <summary>
        /// Tries to sample the height at the specified position. The matrix is resolved from the position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="height">The height at the position.</param>
        /// <returns><c>true</c> if the position represents valid terrain and a height could be found; otherwise <c>false</c></returns>
        bool TrySampleHeight(Vector3 position, out float height);

        /// <summary>
        /// Tries to sample the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="matrix">The matrix in which to sample the height.</param>
        /// <param name="height">The height at the position.</param>
        /// <returns><c>true</c> if the position represents valid terrain and a height could be found; otherwise <c>false</c></returns>
        bool TrySampleHeight(Vector3 position, CellMatrix matrix, out float height);
    }
}
