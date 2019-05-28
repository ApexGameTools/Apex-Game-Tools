/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.DataStructures;
using UnityEngine;

    /// <summary>
    /// Interface for... yes well have a guess
    /// </summary>
    public interface ICellMatrixConfiguration
    {
        /// <summary>
        /// The origin, i.e. center of the grid
        /// </summary>
        Vector3 origin { get; }

        /// <summary>
        /// size along the x-axis.
        /// </summary>
        int sizeX { get; }

        /// <summary>
        /// size along the z-axis.
        /// </summary>
        int sizeZ { get; }

        /// <summary>
        /// The cell size.
        /// </summary>
        float cellSize { get; }

        /// <summary>
        /// The obstacle sensitivity range, meaning any obstacle within this range of the cell center will cause the cell to be blocked.
        /// </summary>
        float obstacleSensitivityRange { get; }

        /// <summary>
        /// Whether or not to generate a height map to enable units to follow a terrain of differing heights.
        /// </summary>
        bool generateHeightmap { get; }

        /// <summary>
        /// Gets the type of the height lookup. Dictionaries are fast but memory hungry. Quad Tree stats depend on height density.
        /// </summary>
        /// <value>
        /// The type of the height lookup.
        /// </value>
        HeightLookupType heightLookupType { get; }

        /// <summary>
        /// Gets the height lookup maximum depth. Only applicable to Quad Trees.
        /// </summary>
        /// <value>
        /// The height lookup maximum depth.
        /// </value>
        int heightLookupMaxDepth { get; }

        /// <summary>
        /// The upper boundary (y - value) of the matrix.
        /// </summary>
        float upperBoundary { get; }

        /// <summary>
        /// The lower boundary (y - value) of the matrix.
        /// </summary>
        float lowerBoundary { get; }

        /// <summary>
        /// The obstacle and ground detection mode used when determining the terrain and obstacles of the grid.
        /// </summary>
        ColliderDetectionMode obstacleAndGroundDetection { get; }

        /// <summary>
        /// Gets the obstacle and ground detector. This can be set to a custom implementation in conjunction with <see cref="obstacleAndGroundDetection"/> set to Custom.
        /// </summary>
        IBlockDetector obstacleAndGroundDetector { get; }
      
        /// <summary>
        /// Gets the grid bounds.
        /// </summary>
        Bounds bounds { get; }
    }
}
