/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    /// <summary>
    /// Represents possible detail levels for height maps.
    /// </summary>
    public enum HeightMapDetailLevel
    {
        /// <summary>
        /// The normal detail level, sufficient for most purposes but slightly inaccurate
        /// </summary>
        Normal,

        /// <summary>
        /// The highest detail level, accurate but slow
        /// </summary>
        High
    }
}
