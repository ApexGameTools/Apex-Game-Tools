/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.Grouping
{
    using System.Collections;
    using Apex.Units;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/Grouping/One Path For All Strategy", 1004)]
    public class OnePathForAllStrategy : MonoBehaviour, IGroupingStrategy<IUnitFacade>, IUnitGroupingStrategyFactory
    {
        public IGrouping<IUnitFacade> CreateGrouping(IEnumerable members)
        {
            return new OnePathForAllGroup(members.ToUnitFacades());
        }

        public TransientGroup<IUnitFacade> CreateGroup(int capacity)
        {
            return new OnePathForAllGroup(capacity);
        }

        public bool BelongsToSameGroup(IUnitFacade lhs, IUnitFacade rhs)
        {
            return true;
        }

        public IGroupingStrategy<IUnitFacade> CreateStrategy()
        {
            return this;
        }
    }
}
