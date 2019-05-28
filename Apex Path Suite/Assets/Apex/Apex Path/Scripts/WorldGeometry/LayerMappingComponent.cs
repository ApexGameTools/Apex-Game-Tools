/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using UnityEngine;

    /// <summary>
    /// Maps Unity layers to the internally used <see cref="Layers"/>
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Apex/Game World/Layer Mapping", 1035)]
    [ApexComponent("Game World")]
    public class LayerMappingComponent : SingleInstanceComponent<LayerMappingComponent>
    {
        /// <summary>
        /// The static obstacle layer mask
        /// </summary>
        [Tooltip("The layer(s) dedicated to static obstacles.")]
        public LayerMask staticObstacleLayer;

        /// <summary>
        /// The terrain layer mask
        /// </summary>
        [Tooltip("The layer(s) dedicated to terrain.")]
        public LayerMask terrainLayer;

        /// <summary>
        /// The unit layer mask
        /// </summary>
        [Tooltip("The layer(s) dedicated to units.")]
        public LayerMask unitLayer;

        private void OnEnable()
        {
            Map();
        }

        private void OnValidate()
        {
            Map();
        }

        internal virtual void Map()
        {
            Layers.blocks = this.staticObstacleLayer;
            Layers.terrain = this.terrainLayer;
            Layers.units = this.unitLayer;
        }
    }
}
