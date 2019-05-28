/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    /// <summary>
    /// A factory for creating an IBlockDetector instance
    /// </summary>
    public interface IBlockDetectorFactory
    {
        /// <summary>
        /// Creates an <see cref="IBlockDetector"/> instance.
        /// </summary>
        /// <returns>The instance</returns>
        IBlockDetector Create();
    }
}
