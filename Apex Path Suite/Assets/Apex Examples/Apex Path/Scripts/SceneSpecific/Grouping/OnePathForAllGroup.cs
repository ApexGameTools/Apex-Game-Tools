/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.Grouping
{
    using System.Collections.Generic;
    using Apex.Services;
    using Apex.Units;
    using UnityEngine;

    public class OnePathForAllGroup : DefaultTransientUnitGroup
    {
        public OnePathForAllGroup(int capacity)
            : base(capacity)
        {
        }

        public OnePathForAllGroup(IUnitFacade[] members)
            : base(members)
        {
        }

        public OnePathForAllGroup(IEnumerable<IUnitFacade> members)
            : base(members)
        {
        }

        protected override void MoveToInternal(Vector3 position, bool append)
        {
            var req = this.modelUnit.CreatePathRequest(position, result =>
                {
                    //Please note that this is simply for example purposes, it will not work properly with regards to replanning
                    //Also the default steering in Apex Path is not geared towards group movement, alternate steering is required for that.
                    var path = result.path;
                    for (int i = 0; i < this.count; i++)
                    {
                        var clone = path.Clone();
                        this[i].MoveAlong(clone);
                    }
                });

            GameServices.pathService.QueueRequest(req);
        }
    }
}
