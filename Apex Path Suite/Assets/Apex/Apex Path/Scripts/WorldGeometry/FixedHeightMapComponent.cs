/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Height map with a fixed height
    /// </summary>
    [AddComponentMenu("Apex/Game World/Fixed Height Map", 1031)]
    [ApexComponent("Game World")]
    public class FixedHeightMapComponent : ExtendedMonoBehaviour, IHeightMap
    {
        /// <summary>
        /// The fixed height
        /// </summary>
        public float height;

        /// <summary>
        /// The bounds of the height map
        /// </summary>
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        /// <summary>
        /// Gets the bounds of the height map.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        Bounds IHeightMap.bounds
        {
            get { return this.bounds; }
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
            get { return 1f; }
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
            return this.height;
        }

        /// <summary>
        /// Tries to sample the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="height">The height at the position.</param>
        /// <returns><c>true</c> if the position is covered by the height map and a height could be found; otherwise <c>false</c></returns>
        public bool TrySampleHeight(Vector3 position, out float height)
        {
            height = this.height;
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
            return this.bounds.Contains(pos);
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            HeightMapManager.instance.RegisterMap(this);
        }

        private void OnDisable()
        {
            HeightMapManager.instance.UnregisterMap(this);
        }
    }
}
