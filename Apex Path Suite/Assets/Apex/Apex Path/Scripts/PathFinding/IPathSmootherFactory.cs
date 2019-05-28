/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    /// <summary>
    /// Interface for path smoother factories.
    /// Implement this on a MonoBehaviour class and attach it to the same GameObject as the <see cref="PathServiceComponent"/> to override the default path smoother.
    /// </summary>
    public interface IPathSmootherFactory
    {
        /// <summary>
        /// Creates the path smoother.
        /// </summary>
        /// <returns>The path smoother</returns>
        ISmoothPaths CreateSmoother();
    }
}
