/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using UnityEngine;

    /// <summary>
    /// Interface for Unit Height Provider factories
    /// </summary>
    public interface IUnitHeightProviderFactory
    {
        /// <summary>
        /// Creates the IUnitHeightProvider instance.
        /// </summary>
        /// <param name="c">The unit's collider.</param>
        /// <returns>The IUnitHeightProvider instance</returns>
        IUnitHeightProvider Create(Collider c);
    }
}
