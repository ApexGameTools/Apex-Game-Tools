/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    /// <summary>
    /// Interface for unit facade factories
    /// </summary>
    public interface IUnitFacadeFactory
    {
        /// <summary>
        /// Creates the unit facade.
        /// </summary>
        /// <returns></returns>
        IUnitFacade CreateUnitFacade();
    }
}
