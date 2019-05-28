/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// A height map that encapsulates Unity Terrain
    /// </summary>
    [AddComponentMenu("Apex/Game World/Terrain Height Map", 1043)]
    [ApexComponent("Game World")]
    public sealed class TerrainHeightMap : MonoBehaviour, IHeightMap
    {
        private Bounds _bounds;
        private float _granularity;

        /// <summary>
        /// The terrain
        /// </summary>
        public Terrain terrain;

        /// <summary>
        /// Gets the bounds of the height map.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        public Bounds bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is grid bound.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grid bound; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool isGridBound
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the granularity of the height map, i.e. the distance between height samples.
        /// </summary>
        /// <value>
        /// The granularity.
        /// </value>
        public float granularity
        {
            get { return _granularity; }
        }

        private void Start()
        {
            //We do this in start instead of Awake in order to allow for this component to be added dynamically (AddComponent) otherwise it is not possible to set the required parameters.
            if (this.terrain == null)
            {
                Debug.LogError("You must assign a Terrain asset to the TerrainHeightMap.");
                this.enabled = false;
                return;
            }

            var data = this.terrain.terrainData;

            var bottomLeft = this.terrain.GetPosition();

            var origin = new Vector3(bottomLeft.x + (data.size.x / 2.0f), bottomLeft.y + (data.size.y / 2.0f), bottomLeft.z + (data.size.z / 2.0f));
            _bounds = new Bounds(origin, data.size);

            var scale = terrain.terrainData.heightmapScale;
            _granularity = (scale.x + scale.z) / 2f;
        }

        private void OnEnable()
        {
            HeightMapManager.instance.RegisterMap(this);
        }

        private void OnDisable()
        {
            HeightMapManager.instance.UnregisterMap(this);
        }

        /// <summary>
        /// Samples the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>
        /// The height at the position
        /// </returns>
        public float SampleHeight(Vector3 position)
        {
            return terrain.SampleHeight(position);
        }

        /// <summary>
        /// Tries to sample the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="height">The height at the position.</param>
        /// <returns><c>true</c> if the position is covered by the height map and a height could be found; otherwise <c>false</c></returns>
        public bool TrySampleHeight(Vector3 position, out float height)
        {
            height = terrain.SampleHeight(position);
            return true;
        }

        /// <summary>
        /// Determines whether the bounds of the height map contains the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        ///   <c>true</c> if the position is contained; otherwise false.
        /// </returns>
        public bool Contains(Vector3 pos)
        {
            return _bounds.Contains(pos);
        }
    }
}
