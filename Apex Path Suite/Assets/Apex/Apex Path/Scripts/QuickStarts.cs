/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex
{
    using System;
    using Debugging;
    using Input;
    using LoadBalancing;
    using PathFinding;
    using Services;
    using Steering;
    using Steering.Behaviours;
    using Steering.Components;
    using Steering.HeightNavigation;
    using Units;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using Utilities;
    using WorldGeometry;

    public static partial class QuickStarts
    {
        public static GameObject GameWorld(GameObject target)
        {
            GameObject gameWorld = null;
            ApexComponentMaster master;

            Component identifier = ComponentHelper.FindFirstComponentInScene<GameServicesInitializerComponent>();
            if (identifier == null)
            {
                identifier = ComponentHelper.FindFirstComponentInScene<LoadBalancerComponent>();
            }

            if (identifier != null)
            {
                gameWorld = identifier.gameObject;
                Debug.Log("Existing game world found, updating that.");
            }
            else if (target != null)
            {
                gameWorld = target;
            }
            else
            {
                gameWorld = new GameObject("Game World");
                Debug.Log("No game world found, creating one.");
            }

            bool toggleAll = gameWorld.AddIfMissing<ApexComponentMaster>(false, out master);

            gameWorld.AddIfMissing<GameServicesInitializerComponent>(true);
            gameWorld.AddIfMissing<NavigationSettingsComponent>(true);

            if (gameWorld.AddIfMissing<GridComponent>(true))
            {
                gameWorld.AddIfMissing<GridVisualizer>(false);
            }

            LayerMappingComponent lmc;
            if (gameWorld.AddIfMissing<LayerMappingComponent>(true, out lmc))
            {
                EnsureLayers(lmc);
            }

            gameWorld.AddIfMissing<PathServiceComponent>(true);
            gameWorld.AddIfMissing<LoadBalancerComponent>(true);

            ExtendGameWorld(gameWorld);

            if (toggleAll)
            {
                master.ToggleAll();
            }

            return gameWorld;
        }

        public static GameObject NavigatingUnit(GameObject target, bool ensureGameworld)
        {
            ApexComponentMaster master;
            Rigidbody rb;

            //Add the required components
            target.AddIfMissing<Rigidbody>(false, out rb);
            bool toggleAll = target.AddIfMissing<ApexComponentMaster>(false, out master);
            target.AddIfMissing<HumanoidSpeedComponent>(false);
            target.AddIfMissing<SteerableUnitComponent>(false);
            target.AddIfMissing<UnitComponent>(false);
            target.AddIfMissing<DefaultHeightNavigator>(false);
            target.AddIfMissing<PathOptionsComponent>(false);
            target.AddIfMissing<SteerToAlignWithVelocity>(false);
            AddIfMissing<SteerForPathComponent>(target, 5);
            target.AddIfMissing<PathVisualizer>(false);

            ExtendNavigatingUnit(target);

            if (toggleAll)
            {
                master.ToggleAll();
            }

            //Adjust components
            if (rb != null)
            {
                rb.constraints |= RigidbodyConstraints.FreezeRotation;
                rb.useGravity = false;
            }

            if (ensureGameworld)
            {
                return GameWorld(null);
            }

            return null;
        }

        public static void NavigatingUnitOnPatrol(GameObject target, bool ensureGameworld)
        {
            var gameWorld = NavigatingUnit(target, ensureGameworld);

            PatrolComponent patroller;
            if (!target.AddIfMissing<PatrolComponent>(false, out patroller))
            {
                return;
            }

            if (gameWorld == null)
            {
                return;
            }

            //Create the route
            var routeIndex = gameWorld.GetComponentsInChildren<PatrolPointsComponent>(true).Length + 1;
            var routeGO = new GameObject("Patrol Route " + routeIndex);
            routeGO.transform.parent = gameWorld.transform;
            var route = routeGO.AddComponent<PatrolPointsComponent>();

            route.points = new Vector3[]
            {
                target.transform.position,
                target.transform.position + (Vector3.forward * 8f)
            };

            patroller.route = route;
        }

        public static void NavigatingUnitWandering(GameObject target, bool ensureGameworld)
        {
            NavigatingUnit(target, ensureGameworld);

            target.AddIfMissing<WanderComponent>(false);
        }

        public static void NavigatingUnitWithSelection(GameObject target, bool ensureGameworld)
        {
            var gameWorld = NavigatingUnit(target, ensureGameworld);

            UnitComponent selectableBehavior;
            target.AddIfMissing<UnitComponent>(false, out selectableBehavior);

            //Add the selection visual
            const string SelectionVisualName = "SelectionVisual";
            GameObject selectVisual;
            var selectVisualTransform = target.transform.Find(SelectionVisualName);
            if (selectVisualTransform == null)
            {
                var mold = Resources.Load<GameObject>("Prefabs/UnitSelectedCustom");
                if (mold == null)
                {
                    mold = Resources.Load<GameObject>("Prefabs/UnitSelected");
                }

                selectVisual = GameObject.Instantiate(mold) as GameObject;
                selectVisual.name = SelectionVisualName;

                selectVisualTransform = selectVisual.transform;
                selectVisualTransform.parent = target.transform;
                selectVisualTransform.localPosition = Vector3.zero;
                selectVisualTransform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            }
            else
            {
                selectVisual = selectVisualTransform.gameObject;
            }

            selectVisual.SetActive(false);
            selectableBehavior.selectionVisual = selectVisual;
            selectableBehavior.isSelectable = true;

            if (gameWorld == null)
            {
                return;
            }

            if (gameWorld.GetComponentInChildren<SelectionRectangleComponent>() == null)
            {
                var mold = Resources.Load<GameObject>("Prefabs/SelectionRectCustom");
                if (mold == null)
                {
                    mold = Resources.Load<GameObject>("Prefabs/SelectionRect");
                }

                var selectionRect = GameObject.Instantiate(mold) as GameObject;
                selectionRect.transform.parent = gameWorld.transform;
            }

            AddIfMissing<InputReceiverBasic, InputReceiverAttribute>(gameWorld, true);
        }

        public static void BasicAvoidance(GameObject target, bool ensureGameworld)
        {
            NavigatingUnit(target, ensureGameworld);

            target.AddIfMissing<BasicScanner>(false);
            AddIfMissing<SteerForBasicAvoidanceComponent>(target, 5);
        }

        public static GameObject Portal(GameObject target)
        {
            var portalIndex = target.GetComponentsInChildren<GridPortalComponent>(true).Length + 1;

            var parent = target.transform;
            target = new GameObject("Portal " + portalIndex);
            target.transform.parent = parent;
            target.transform.localPosition = Vector3.zero;

            target.AddComponent<GridPortalComponent>();

            return target;
        }

        public static GameObject PatrolRoute(GameObject target)
        {
            var routeIndex = target.GetComponentsInChildren<PatrolPointsComponent>(true).Length + 1;
            var routeGO = new GameObject("Patrol Route " + routeIndex);
            routeGO.transform.parent = target.transform;
            routeGO.transform.localPosition = Vector3.zero;

            routeGO.AddComponent<PatrolPointsComponent>();
            return routeGO;
        }

        static partial void ExtendGameWorld(GameObject gameWorld);

        static partial void ExtendNavigatingUnit(GameObject target);

        /// <summary>
        /// Adds a steering component of the specified type if it does not already exist
        /// </summary>
        /// <typeparam name="T">The type of component to add</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <param name="priority">The priority.</param>
        /// <returns><c>true</c> if the component was added, otherwise <c>false</c></returns>
        private static bool AddIfMissing<T>(GameObject target, int priority) where T : SteeringComponent
        {
            T component;
            if (target.AddIfMissing(false, out component))
            {
                component.priority = priority;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a component of the specified type if it does not already exist
        /// </summary>
        /// <typeparam name="T">>The type of component to add</typeparam>
        /// <typeparam name="TMarkerAttribute">The type of the marker attribute that identifies the component type</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <param name="globalSearch">if set to <c>true</c> the check to see if the component already exists will be done in the entire project, otherwise it will check the <paramref name="target"/>.</param>
        /// <param name="component">The component that was added.</param>
        /// <returns><c>true</c> if the component was added, otherwise <c>false</c></returns>
        private static bool AddIfMissing<T, TMarkerAttribute>(GameObject target, bool globalSearch, out T component)
            where T : Component
            where TMarkerAttribute : Attribute
        {
            component = null;
            UnityEngine.Object[] candidates;
            if (globalSearch)
            {
                candidates = UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour));
            }
            else
            {
                candidates = target.GetComponents<MonoBehaviour>();
            }

            foreach (var mb in candidates)
            {
#if NETFX_CORE
                //For some reason this causes an issue with Win8 store, even though it is the exact same code as elsewhere.
                //It is however totally irrelevant since this is actually editor stuff.
                //var typeInf = mb.GetType().GetTypeInfo();
                //if (typeInf.CustomAttributes.Any(a => a.AttributeType == typeof(TMarkerAttribute)))
                //{
                //    return false;
                //}
#else
                if (Attribute.IsDefined(mb.GetType(), typeof(TMarkerAttribute)))
                {
                    return false;
                }
#endif
            }

            component = target.AddComponent<T>();
            return true;
        }

        /// <summary>
        /// Adds a component of the specified type if it does not already exist
        /// </summary>
        /// <typeparam name="T">>The type of component to add</typeparam>
        /// <typeparam name="TMarkerAttribute">The type of the marker attribute that identifies the component type</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <param name="globalSearch">if set to <c>true</c> the check to see if the component already exists will be done in the entire project, otherwise it will check the <paramref name="target"/>.</param>
        /// <returns><c>true</c> if the component was added, otherwise <c>false</c></returns>
        private static bool AddIfMissing<T, TMarkerAttribute>(GameObject target, bool globalSearch)
            where T : Component
            where TMarkerAttribute : Attribute
        {
            T component;
            return AddIfMissing<T, TMarkerAttribute>(target, globalSearch, out component);
        }

        private static void EnsureLayers(LayerMappingComponent lmc)
        {
#if UNITY_EDITOR
            var arr = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (arr == null || arr.Length == 0)
            {
                Debug.LogWarning("Unable to set up default layers, please ensure that layer mapping is properly set.");
                return;
            }

            var tagManagerAsset = new SerializedObject(arr[0]);

            lmc.staticObstacleLayer = EnsureLayer("StaticObstacles", tagManagerAsset);
            lmc.terrainLayer = EnsureLayer("Terrain", tagManagerAsset);
            lmc.unitLayer = EnsureLayer("Units", tagManagerAsset);
            lmc.Map();
#else
            Debug.LogWarning("Unable to set up default layers, please ensure that layer mapping is properly set.");
#endif
        }
#if UNITY_EDITOR
        private static int EnsureLayer(string name, SerializedObject tagManagerAsset)
        {
#if UNITY_5 || UNITY_2017
            SerializedProperty layersProp = tagManagerAsset.FindProperty("layers");
#endif
            int firstVacant = -1;
            SerializedProperty firstVacantProp = null;
            for (int i = 8; i <= 31; i++)
            {
#if UNITY_5 || UNITY_2017
                var sp = layersProp.GetArrayElementAtIndex(i);
#else
                var layerPropName = "User Layer " + i;
                var sp = tagManagerAsset.FindProperty(layerPropName);
#endif
                if (sp == null)
                {
                    continue;
                }
                else if (name.Equals(sp.stringValue))
                {
                    return 1 << i;
                }
                else if (firstVacant < 0 && string.IsNullOrEmpty(sp.stringValue))
                {
                    firstVacant = i;
                    firstVacantProp = sp;
                }
            }

            if (firstVacant < 0)
            {
                Debug.LogWarning(string.Format("Unable to add a {0} layer, please ensure that the Layer Mapping on the Game World is correct.", name));
                return 0;
            }
            else
            {
                Debug.Log(string.Format("Added a {0} layer.", name));
                firstVacantProp.stringValue = name;
                tagManagerAsset.ApplyModifiedProperties();

                return 1 << firstVacant;
            }
        }
#endif
    }
}
