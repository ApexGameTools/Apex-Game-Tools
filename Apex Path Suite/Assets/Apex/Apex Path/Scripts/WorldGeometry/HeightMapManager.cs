/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages all <see cref="IHeightMap"/>s in the game world.
    /// </summary>
    public class HeightMapManager
    {
        /// <summary>
        /// The singleton instance
        /// </summary>
        public static readonly HeightMapManager instance = new HeightMapManager();

        private static readonly IHeightMap _zero = new ZeroHeightMap();
        private List<IHeightMap> _offGridHeightMaps;
        private List<IHeightMap> _onGridHeightMaps;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightMapManager"/> class.
        /// </summary>
        public HeightMapManager()
        {
            _offGridHeightMaps = new List<IHeightMap>();
            _onGridHeightMaps = new List<IHeightMap>();
        }

        /// <summary>
        /// Gets a value indicating whether height maps are enabled, i.e. if at least one height map exists in the scene
        /// </summary>
        /// <value>
        /// <c>true</c> if height maps are enabled; otherwise, <c>false</c>.
        /// </value>
        public bool areHeightMapsEnabled
        {
            get { return _onGridHeightMaps.Count > 0 || _offGridHeightMaps.Count > 0; }
        }

        /// <summary>
        /// Registers the height map with this manager.
        /// </summary>
        /// <param name="heightMap">The height map.</param>
        public void RegisterMap(IHeightMap heightMap)
        {
            if (heightMap.isGridBound)
            {
                _onGridHeightMaps.AddUnique(heightMap);
            }
            else
            {
                _offGridHeightMaps.AddUnique(heightMap);
            }
        }

        /// <summary>
        /// Unregisters the height map with this manager.
        /// </summary>
        /// <param name="heightMap">The height map.</param>
        public void UnregisterMap(IHeightMap heightMap)
        {
            if (heightMap.isGridBound)
            {
                _onGridHeightMaps.Remove(heightMap);
            }
            else
            {
                _offGridHeightMaps.Remove(heightMap);
            }
        }

        /// <summary>
        /// Gets the height map at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        /// The matching height map or null if no match is found.
        /// </returns>
        public IHeightMap GetHeightMap(Vector3 pos)
        {
            var count = _onGridHeightMaps.Count;
            for (int i = 0; i < count; i++)
            {
                var hm = _onGridHeightMaps[i];

                if (hm.bounds.Contains(pos))
                {
                    return hm;
                }
            }

            count = _offGridHeightMaps.Count;
            for (int i = 0; i < count; i++)
            {
                var hm = _offGridHeightMaps[i];

                if (hm.bounds.Contains(pos))
                {
                    return hm;
                }
            }

            return _zero;
        }
    }
}
