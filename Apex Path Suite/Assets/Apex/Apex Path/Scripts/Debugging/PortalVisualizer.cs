/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using UnityEngine;

    /// <summary>
    /// Visualizer for <see cref="Apex.WorldGeometry.GridPortal"/>s. This visualizer does not in fact draw anything it is simply a global controller for all portals.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Apex/Game World/Debugging/Portal Visualizer", 1015)]
    [ApexComponent("Debugging")]
    public sealed class PortalVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Whether to always draw the visualization gizmos, even when the gameobject is not selected.
        /// </summary>
        [Tooltip("Whether to always draw the visualization gizmos for all portals in the scene.\n\nIf checked all portals are drawn regardless of their individual setting.\n\nIf unchecked each portal's individual setting is used.")]
        public bool drawAllPortals = true;

        private static PortalVisualizer _instance;
        private static bool _initialized;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static PortalVisualizer instance
        {
            get
            {
                if (!_initialized && _instance == null)
                {
                    _initialized = true;
                    _instance = FindObjectOfType<PortalVisualizer>();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Debug.Log("A Portal Visualizer already exists in the scene!");

                if (Application.isPlaying)
                {
                    Destroy(this);
                }
                else
                {
                    DestroyImmediate(this);
                }
            }
            else
            {
                _initialized = true;
                _instance = this;
            }
        }
    }
}
