/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Interface for height lookups
    /// </summary>
    public interface IHeightLookup
    {
        /// <summary>
        /// Gets a value indicating whether this instance has heights.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has heights; otherwise, <c>false</c>.
        /// </value>
        bool hasHeights { get; }

        /// <summary>
        /// Gets the height count.
        /// </summary>
        /// <value>
        /// The height count.
        /// </value>
        int heightCount { get; }

        /// <summary>
        /// Adds a height at the height matrix x and z position.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="z">The z.</param>
        /// <param name="height">The height.</param>
        /// <returns><c>true</c> if the height was added; otherwise <c>false</c>.</returns>
        bool Add(int x, int z, float height);

        /// <summary>
        /// Tries to get the height at the height matrix x and z position.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="z">The z.</param>
        /// <param name="height">The height.</param>
        /// <returns><c>true</c> if the height was found; otherwise <c>false</c>.</returns>
        bool TryGetHeight(int x, int z, out float height);

        /// <summary>
        /// Cleanups this lookup.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Prepares for update.
        /// </summary>
        /// <param name="suggestedBounds">The suggested bounds.</param>
        /// <param name="requiredBounds">The required bounds.</param>
        /// <returns>The height lookup ready for updates</returns>
        IHeightLookup PrepareForUpdate(MatrixBounds suggestedBounds, out MatrixBounds requiredBounds);

        /// <summary>
        /// Finishes the update.
        /// </summary>
        /// <param name="updatedHeights">The updated heights.</param>
        void FinishUpdate(IHeightLookup updatedHeights);

        /// <summary>
        /// Renders a graphical representation of the height lookup
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="pointGranularity">The point granularity.</param>
        /// <param name="drawColor">Color to draw with.</param>
        void Render(Vector3 position, float pointGranularity, Color drawColor);
    }
}
