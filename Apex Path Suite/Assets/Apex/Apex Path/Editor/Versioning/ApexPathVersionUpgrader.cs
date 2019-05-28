/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor.Versioning
{
    using System.Collections.Generic;
    using System.Linq;
    using Apex;
    using Debugging;
    using LoadBalancing;
    using PathFinding;
    using Services;
    using Steering;
    using Steering.Components;
    using Steering.HeightNavigation;
    using Units;
    using UnityEngine;
    using Utilities;
    using WorldGeometry;

#pragma warning disable 0618
    public class ApexPathVersionUpgrader : VersionUpgradeAction
    {
        public override bool Upgrade()
        {
            bool changed = false;

            changed |= Replace<ApexComponentMasterOld, ApexComponentMaster>(
                (a, b) =>
                {
                    SetPrivate(b, "_firstTime", a._firstTime);
                });

            changed |= Replace<LoadBalancerComponentOld, LoadBalancerComponent>(
                (a, b) =>
                {
                    SetPrivate(b, "_configurations", a._configurations);
                    SetPrivate(b, "_mashallerMaxMillisecondPerFrame", a._mashallerMaxMillisecondPerFrame);
                });

            changed |= Replace<LoadBalancerPerformanceVisualizerOld, LoadBalancerPerformanceVisualizer>();

            changed |= Replace<TurnableUnitComponent, SteerToAlignWithVelocity>(
                (a, b) =>
                {
                    b.alignWithElevation = (a.ignoreAxis == WorldGeometry.Axis.None);
                });

            var nscAdded = false;
            var nsc = AddGameWorldItem<NavigationSettingsComponent>(
                (gw, ns, added) =>
                {
                    nscAdded = added;
                    if (added)
                    {
                        var grid = ComponentHelper.FindFirstComponentInScene<GridComponent>();
                        if (grid == null)
                        {
                            return;
                        }

                        ns.heightSamplingGranularity = grid.heightGranularity;

                        var unitnav = ns.unitsHeightNavigationCapability;
                        unitnav.maxClimbHeight = grid.maxScaleHeight;
                        unitnav.maxSlopeAngle = grid.maxWalkableSlopeAngle;
                        ns.unitsHeightNavigationCapability = unitnav;
                    }

                    //Fix old sphere cast option
                    if ((int)ns.heightSampling == 3)
                    {
                        ns.heightSampling = HeightSamplingMode.Raycast;
                        changed = true;
                    }
                });

            changed |= nscAdded;

            //Ensure each unit has a height navigator
            if (nscAdded && nsc.heightSampling != HeightSamplingMode.NoHeightSampling)
            {
                var suc = GetAllNonPrefabInstances<SteerableUnitComponent>();
                foreach (var c in suc)
                {
                    if (c.As<IHeightNavigator>() == null)
                    {
                        var dhn = c.gameObject.AddComponent<DefaultHeightNavigator>();

                        dhn.gravity = c.gravity;
                        dhn.groundStickynessFactor = c.groundStickynessFactor;
                        dhn.terminalVelocity = c.terminalVelocity;

                        DefaultHeightNavigatorEditor.EnsureValidProvider(dhn, nsc.heightSampling);

                        //Set the default constraints of the rigidbody, we no longer want to force this on awake
                        var rb = c.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.constraints |= RigidbodyConstraints.FreezeRotation;
                        }
                    }
                }
            }

            //Get path finder options from steer for path
            var sfp = GetAllNonPrefabInstances<SteerForPathComponent>();
            changed |= FixPathOptions(sfp);

            //Get selection visual from selectable unit if present
            var selectables = GetAllNonPrefabInstances<SelectableUnitComponent>();
            changed |= FixSelectables(selectables);

            //Set any basic avoidance to the same prio as steer for path
            var sfba = GetAllNonPrefabInstances<SteerForBasicAvoidanceComponent>();
            foreach (var a in sfba)
            {
                if (a.priority == 0)
                {
                    a.priority = 5;
                    changed = true;
                }
            }

            var units = GetAllNonPrefabInstances<UnitComponent>();
            ApexComponentMaster m;
            foreach (var u in units)
            {
                if (u.gameObject.AddIfMissing<ApexComponentMaster>(false, out m))
                {
                    changed = true;
                    while (UnityEditorInternal.ComponentUtility.MoveComponentUp(m))
                    {
                        /* NOOP */
                    }
                }
            }

            AddGameWorldItem<ApexComponentMaster>((gw, acm, added) => changed |= added);

            return changed;
        }

        private static bool FixPathOptions(IEnumerable<SteerForPathComponent> sfp)
        {
            bool changed = false;
            foreach (var source in sfp)
            {
                if (source.priority == 0)
                {
                    source.priority = 5;
                    changed = true;
                }

                var go = source.gameObject;

                PathOptionsComponent po;
                if (go.AddIfMissing<PathOptionsComponent>(false, out po))
                {
                    po.allowCornerCutting = source.allowCornerCutting;
                    po.maxEscapeCellDistanceIfOriginBlocked = source.maxEscapeCellDistanceIfOriginBlocked;
                    po.navigateToNearestIfBlocked = source.navigateToNearestIfBlocked;
                    po.pathingPriority = source.pathingPriority;
                    po.preventDiagonalMoves = source.preventDiagonalMoves;
                    po.usePathSmoothing = source.usePathSmoothing;
                    po.replanInterval = source.replanInterval;
                    po.replanMode = source.replanMode;
                    po.requestNextWaypointDistance = source.requestNextWaypointDistance;
                    po.nextNodeDistance = source.nextNodeDistance;
                    po.announceAllNodes = source.announceAllNodes;

                    changed = true;
                }
            }

            return changed;
        }

        private static bool FixSelectables(IEnumerable<SelectableUnitComponent> selectables)
        {
            //important to do that here and not after the iteration, since they are destroyed.
            bool changed = selectables.Any();
            foreach (var source in selectables)
            {
                var go = source.gameObject;

                var unit = go.GetComponent<UnitComponent>();
                unit.isSelectable = true;
                unit.selectionVisual = source.selectionVisual;

                Component.DestroyImmediate(source, true);
            }

            return changed;
        }

        private static T AddGameWorldItem<T>(Initializer<T> init = null) where T : Component
        {
            GameObject gameWorld = null;

            var servicesInitializer = ComponentHelper.FindFirstComponentInScene<GameServicesInitializerComponent>();
            if (servicesInitializer != null)
            {
                gameWorld = servicesInitializer.gameObject;

                T item;
                var added = gameWorld.AddIfMissing<T>(false, out item);

                if (init != null)
                {
                    init(gameWorld, item, added);
                }

                return item;
            }

            return null;
        }
    }
}
