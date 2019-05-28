/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    /// <summary>
    /// Provider types for unit height navigation.
    /// </summary>
    public enum ProviderType
    {
        /// <summary>
        /// No provider...
        /// </summary>
        None = 0,

        /// <summary>
        /// No sampling but gravity is active.
        /// </summary>
        ZeroHeight = 1,

        /// <summary>
        /// "Samples corners and mid. Reasonably fast but not very accurate.
        /// </summary>
        HeightMapBoxFivePoint = 10,

        /// <summary>
        /// Samples front, mid and back. Fast but not very accurate.
        /// </summary>
        HeightMapBoxThreePoint = 11,

        /// <summary>
        /// Samples front, mid and back.\nFast but not very accurate.
        /// </summary>
        HeightMapSphericalThreePoint = 12,

        /// <summary>
        /// Samples corners and mid.\nMid range performance but more accurate than height map.
        /// </summary>
        RaycastBoxFivePoint = 20,

        /// <summary>
        /// Samples front, mid and back. Mid range performance but more accurate than height map.
        /// </summary>
        RaycastBoxThreePoint = 21,

        /// <summary>
        /// Samples front, mid and back.\nMid range performance but more accurate than height map.
        /// </summary>
        RaycastSphericalThreePoint = 22,

        /// <summary>
        /// Highly accurate sampling, but relatively low performance. Only available for spheres and vertical capsules.
        /// </summary>
        SphereCast = 30,

        /// <summary>
        /// Highly accurate sampling, but relatively low performance. Only available for capsules.
        /// </summary>
        CapsuleCast = 40,

        /// <summary>
        /// Your own implementation.
        /// </summary>
        Custom = 100
    }
}
