/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor.Versioning
{
    using System;
    using System.Linq;
    using Steering.Behaviours;
    using Steering.Props;
    using UnityEngine;

    public sealed class ApexPathPatrolUpgrader : VersionUpgradeAction
    {
        public ApexPathPatrolUpgrader()
        {
            this.isOptional = true;
            this.name = "Migrate old patrol behaviours and routes.";
        }

        public override bool Upgrade()
        {
            var oldPatrollers = GetAllNonPrefabInstances<PatrolBehaviour>().ToArray();
            var newPatrollers = new PatrolComponent[oldPatrollers.Length];
            for (int i = 0; i < oldPatrollers.Length; i++)
            {
                newPatrollers[i] = oldPatrollers[i].gameObject.AddComponent<PatrolComponent>();
                newPatrollers[i].randomize = oldPatrollers[i].randomize;
                newPatrollers[i].reverse = oldPatrollers[i].reverseRoute;
                newPatrollers[i].lingerForSeconds = oldPatrollers[i].lingerAtNodesForSeconds;
            }

            var oldRoutes = GetAllNonPrefabInstances<PatrolRoute>().ToArray();
            foreach (var old in oldRoutes)
            {
                var routeGO = old.gameObject;

                var oldPatrolPoints = routeGO.GetComponentsInChildren<PatrolPoint>();
                Array.Sort(
                    oldPatrolPoints,
                    (a, b) =>
                    {
                        var c = a.orderIndex.CompareTo(b.orderIndex);
                        if (c == 0)
                        {
                            return a.gameObject.name.CompareTo(b.gameObject.name);
                        }

                        return c;
                    });

                var newRoute = routeGO.AddComponent<PatrolPointsComponent>();

                newRoute.pointColor = old.gizmoColor;

                var newPoints = newRoute.points = new Vector3[oldPatrolPoints.Length];
                for (int i = 0; i < oldPatrolPoints.Length; i++)
                {
                    newPoints[i] = oldPatrolPoints[i].position;
                    UnityEngine.Object.DestroyImmediate(oldPatrolPoints[i], true);
                }

                for (int i = 0; i < oldPatrollers.Length; i++)
                {
                    if (oldPatrollers[i].route == old)
                    {
                        newPatrollers[i].route = newRoute;
                        break;
                    }
                }
            }

            for (int i = 0; i < oldPatrollers.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(oldPatrollers[i], true);
            }

            for (int i = 0; i < oldRoutes.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(oldRoutes[i], true);
            }

            return (oldPatrollers.Length > 0 || oldRoutes.Length > 0);
        }
    }
}
