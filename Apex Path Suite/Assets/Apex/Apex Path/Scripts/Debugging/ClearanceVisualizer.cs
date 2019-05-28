/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Simple Visualizer for showing grid clearance values
    /// </summary>
    [AddComponentMenu("Apex/Game World/Debugging/Clearance Visualizer", 1011)]
    [ApexComponent("Debugging")]
    public sealed class ClearanceVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The color of the clearance text.
        /// </summary>
        [Tooltip("The color of the clearance text.")]
        public Color textColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

        /// <summary>
        /// "The font size of the clearance text.
        /// </summary>
        [Tooltip("The font size of the clearance text.")]
        public int fontSize = 10;

        private GUIStyle _style;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start()
        {
            /* NOOP but required to be able to disable */
        }

        private void OnGUI()
        {
            if (Camera.current == null || !Application.isPlaying)
            {
                return;
            }

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = this.fontSize
                };
            }

            var grids = FindObjectsOfType<GridComponent>();

            if (grids != null)
            {
                foreach (var gridComp in grids)
                {
                    var grid = gridComp.grid;
                    if (grid == null)
                    {
                        continue;
                    }

                    var matrix = grid.cellMatrix;
                    for (int x = 0; x < grid.sizeX; x++)
                    {
                        for (int z = 0; z < grid.sizeZ; z++)
                        {
                            var c = matrix[x, z] as IHaveClearance;
                            if (c == null || c.clearance <= 0f)
                            {
                                continue;
                            }

                            Vector3 pos = Camera.current.WorldToScreenPoint(matrix[x, z].position);
                            pos.y = Screen.height - pos.y;
                            GUI.color = this.textColor;
                            GUI.Label(new Rect(pos.x - 5f, pos.y - 10f, 50f, 20f), c.clearance.ToString(), _style);
                        }
                    }
                }
            }
        }
    }
}
