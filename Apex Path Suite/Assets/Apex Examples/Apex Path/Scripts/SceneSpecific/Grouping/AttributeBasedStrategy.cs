/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.Grouping
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.Common;
    using Apex.Units;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/Grouping/Attribute Based Strategy", 1002)]
    public class AttributeBasedStrategy : MonoBehaviour, IGroupingStrategy<IUnitFacade>, IUnitGroupingStrategyFactory
    {
        public bool useLINQ = true;

        public IGrouping<IUnitFacade> CreateGrouping(IEnumerable members)
        {
            var units = members.ToUnitFacades();

            if (this.useLINQ)
            {
                //A somewhat cleaner solution, albeit due to the lack of covariance in .Net 3.5 we have to explicitly cast at the end.
                var groups = from u in units
                             group u by u.attributes into grps
                             select new OnePathForAllGroup(grps) as TransientGroup<IUnitFacade>;

                return new Grouping<IUnitFacade>(groups);
            }
            else
            {
                var grpDict = new Dictionary<AttributeMask, OnePathForAllGroup>();

                var grouping = new Grouping<IUnitFacade>(1);
                foreach (var unit in units)
                {
                    OnePathForAllGroup grp;
                    if (!grpDict.TryGetValue(unit.attributes, out grp))
                    {
                        grp = new OnePathForAllGroup(1);
                        grpDict[unit.attributes] = grp;
                        grouping.Add(grp);
                    }

                    grp.Add(unit);
                }

                return grouping;
            }
        }

        public TransientGroup<IUnitFacade> CreateGroup(int capacity)
        {
            return new OnePathForAllGroup(capacity);
        }

        public bool BelongsToSameGroup(IUnitFacade lhs, IUnitFacade rhs)
        {
            return lhs.attributes == rhs.attributes;
        }

        public IGroupingStrategy<IUnitFacade> CreateStrategy()
        {
            return this;
        }
    }
}
