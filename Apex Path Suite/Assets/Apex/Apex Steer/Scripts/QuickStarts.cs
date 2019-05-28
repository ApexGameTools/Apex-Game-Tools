/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex
{
    using Steering;
    using Steering.Components;
    using Steering.VectorFields;
    using Units;
    using UnityEngine;

    public static partial class QuickStarts
    {
        public static void VectorFieldNavigation(GameObject target, bool ensureGameworld)
        {
            var gameWorld = NavigatingUnit(target, ensureGameworld);

            AddIfMissing<SteerForVectorFieldComponent>(target, 5);
            AddIfMissing<SteerForFormationComponent>(target, 10);
            AddIfMissing<SteerForBlockedCellRepulsionComponent>(target, 20);
            AddIfMissing<SteerForContainmentComponent>(target, 20);
            target.AddIfMissing<SteeringController>(false);

            if (gameWorld == null)
            {
                return;
            }

            gameWorld.AddIfMissing<VectorFieldManagerComponent>(true);

            if (gameWorld.As<IUnitGroupingStrategyFactory>() == null)
            {
                gameWorld.AddComponent<SteeringUnitGroupingStrategyFactory>();
            }
        }

        static partial void ExtendGameWorld(GameObject gameWorld)
        {
            gameWorld.AddIfMissing<VectorFieldManagerComponent>(true);

            if (gameWorld.As<IUnitGroupingStrategyFactory>() == null)
            {
                gameWorld.AddComponent<SteeringUnitGroupingStrategyFactory>();
            }
        }

        static partial void ExtendNavigatingUnit(GameObject target)
        {
            target.NukeSingle<BasicScanner>();
            target.NukeSingle<SteerForBasicAvoidanceComponent>();

            target.AddIfMissing<SteeringScanner>(false);
            AddIfMissing<SteerForSeparationComponent>(target, 9);
            AddIfMissing<SteerForUnitAvoidanceComponent>(target, 15);
        }
    }
}
