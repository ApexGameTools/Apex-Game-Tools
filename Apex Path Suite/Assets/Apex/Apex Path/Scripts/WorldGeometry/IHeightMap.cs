/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Interface for height maps
    /// </summary>
    public interface IHeightMap
    {
        /// <summary>
        /// Gets the bounds of the height map.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        Bounds bounds { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is grid bound.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grid bound; otherwise, <c>false</c>.
        /// </value>
        bool isGridBound { get; }

        /// <summary>
        /// Gets the granularity of the height map, i.e. the distance between height samples.
        /// </summary>
        /// <value>
        /// The granularity.
        /// </value>
        float granularity { get; }

        /// <summary>
        /// Samples the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The height at the position</returns>
        float SampleHeight(Vector3 position);

        /// <summary>
        /// Tries to sample the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="height">The height at the position.</param>
        /// <returns><c>true</c> if the position is covered by the height map and a height could be found; otherwise <c>false</c></returns>
        bool TrySampleHeight(Vector3 position, out float height);

        /// <summary>
        /// Determines whether the bounds of the height map contains the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns><c>true</c> if the position is contained; otherwise false.</returns>
        bool Contains(Vector3 pos);
    }
}
