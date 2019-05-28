/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    using Apex.GameState;
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Component that initializes essential game services.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Game Services Initializer", 1032)]
    [ApexComponent("Game World")]
    public partial class GameServicesInitializerComponent : SingleInstanceComponent<GameServicesInitializerComponent>
    {
        /// <summary>
        /// Initializes the services.
        /// </summary>
        protected virtual void InitializeServices()
        {
            var messageBusFactory = this.As<IMessageBusFactory>();
            if (messageBusFactory == null)
            {
                GameServices.messageBus = new BasicMessageBus();
            }
            else
            {
                GameServices.messageBus = messageBusFactory.CreateMessageBus();
            }

            //The game state manager relies on the message bus so it must be initialized after that
            GameServices.gameStateManager = new GameStateManager(this.As<IUnitFacadeFactory>());

            //Get the grouping strategy for units registered
            var stratFactory = this.As<IUnitGroupingStrategyFactory>();
            if (stratFactory != null)
            {
                GroupingManager.RegisterGroupingStrategy(stratFactory.CreateStrategy());
            }
            else
            {
                GroupingManager.RegisterGroupingStrategy(new DefaultUnitGroupingStrategy());
            }

            //Set the cell cost strategy
            var cellCostStrategyFactory = this.As<ICellCostStrategyFactory>();
            if (cellCostStrategyFactory == null)
            {
                GameServices.cellCostStrategy = new DefaultCellCostStrategy();
            }
            else
            {
                GameServices.cellCostStrategy = cellCostStrategyFactory.CreateCostStrategy();
            }
        }

        /// <summary>
        /// Initializes certain components that can be added automatically based on other scene elements.
        /// </summary>
        protected virtual void InitializeAutoComponents()
        {
            var terrainMaps = FindObjectsOfType<TerrainHeightMap>();
            if (terrainMaps.Length == 0)
            {
                var terrains = FindObjectsOfType<Terrain>();
                foreach (var t in terrains)
                {
                    var hm = this.gameObject.AddComponent<TerrainHeightMap>();
                    hm.terrain = t;
                }
            }
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        protected sealed override void OnAwake()
        {
            InitializeServices();

            InitializeAutoComponents();
        }
    }
}
