/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using UnityEngine;

    /// <summary>
    /// Default Steering class for registering the <see cref="DefaultSteeringUnitGroupingStrategy" /> as the chosen grouping strategy.
    /// Write your own <see cref="IUnitGroupingStrategyFactory" /> to register a different grouping strategy.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Default Unit Grouping Strategy", 1029)]
    [ApexComponent("Steering")]
    public class SteeringUnitGroupingStrategyFactory : MonoBehaviour, IUnitGroupingStrategyFactory
    {
        /// <summary>
        /// Creates the unit grouping strategy for <see cref="IUnitFacade"/>-based units.
        /// </summary>
        /// <returns>Returns a new instance of <see cref="IGroupingStrategy{IUnitFacade}"/></returns>
        public IGroupingStrategy<IUnitFacade> CreateStrategy()
        {
            return new DefaultSteeringUnitGroupingStrategy();
        }
    }
}