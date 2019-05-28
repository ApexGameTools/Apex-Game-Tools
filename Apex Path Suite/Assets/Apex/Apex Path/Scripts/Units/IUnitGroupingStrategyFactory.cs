/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    /// <summary>
    /// A class responsible for registering and creating a unit grouping strategy.
    /// </summary>
    public interface IUnitGroupingStrategyFactory
    {
        /// <summary>
        /// Creates the unit grouping strategy for <see cref="IUnitFacade"/>-based units.
        /// </summary>
        /// <returns>Returns a new instance of <see cref="IGroupingStrategy{IUnitFacade}"/></returns>
        IGroupingStrategy<IUnitFacade> CreateStrategy();
    }
}