/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Marks an area that is otherwise traversable to be invalid as an actual destination, i.e. navigating units will not be allowed to end their path here.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Invalid Destination", 1034)]
    [ApexComponent("Game World")]
    public class InvalidDestinationComponent : MonoBehaviour
    {
        /// <summary>
        /// Whether the entire transform should be considered an invalid destination. If set to <c>false</c> the <see cref="onlySubArea"/> is used.
        /// </summary>
        public bool entireTransform = true;

        /// <summary>
        /// If <see cref="entireTransform"/> is <c>false</c>, this area defines the invalid destination.
        /// </summary>
        public Bounds onlySubArea;

        private void Start()
        {
            if (this.gameObject.layer == 0)
            {
                Debug.LogWarning("The InvalidDestinationComponent must be assigned to the terrain layer to function correctly.");
                this.enabled = false;
            }
        }
    }
}
