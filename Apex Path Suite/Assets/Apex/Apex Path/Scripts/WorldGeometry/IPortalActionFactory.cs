/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    /// <summary>
    /// Interface for <see cref="IPortalAction"/> factories.
    /// </summary>
    public interface IPortalActionFactory
    {
        /// <summary>
        /// Creates the portal action.
        /// </summary>
        /// <returns>The portal action</returns>
        IPortalAction Create();
    }
}
