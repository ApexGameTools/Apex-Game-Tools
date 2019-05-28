/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    using Apex.Services;
    using UnityEngine;

    /// <summary>
    /// Attaching this component to a unit makes it selectable.
    /// </summary>
    [AddComponentMenu("")]
    public class SelectableUnitComponent : ExtendedMonoBehaviour
    {
        /// <summary>
        /// The visual used to indicate whether the unit is selected or not.
        /// </summary>
        public GameObject selectionVisual;

        private void Awake()
        {
            Debug.LogWarning("The SelectableUnitComponent component is no longer valid and should be removed. Use the 'Upgrade Scene' tool in the Tools -> Apex menu to fix up the scene.");
        }
    }
}
