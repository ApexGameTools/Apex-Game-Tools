/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    /// <summary>
    /// The algorithms for slowing movement / rotation to a halt
    /// </summary>
    public enum SlowingAlgorithm
    {
        /// <summary>
        /// Linear slow down
        /// </summary>
        Linear,

        /// <summary>
        /// Logarithmic slow down
        /// </summary>
        Logarithmic
    }
}
