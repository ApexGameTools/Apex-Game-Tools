/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using UnityEngine;

    /// <summary>
    /// Defines the layers used by various services
    /// </summary>
    public static partial class Layers
    {
        /// <summary>
        /// Gets or sets the terrain layer mask.
        /// </summary>
        /// <value>
        /// The terrain.
        /// </value>
        public static LayerMask terrain { get; set; }

        /// <summary>
        /// Gets or sets the blocks layer mask.
        /// </summary>
        /// <value>
        /// The blocks.
        /// </value>
        public static LayerMask blocks { get; set; }

        /// <summary>
        /// Gets or sets the units layer mask.
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public static LayerMask units { get; set; }

        /// <summary>
        /// Check if a game object is in a particular layer.
        /// </summary>
        /// <param name="go">The Game Object.</param>
        /// <param name="layer">The layer(s).</param>
        /// <returns><c>true</c> if the Game Object is in the specified layer; otherwise <c>false</c></returns>
        public static bool InLayer(GameObject go, LayerMask layer)
        {
            return (layer & (1 << go.layer)) != 0;
        }
    }
}
