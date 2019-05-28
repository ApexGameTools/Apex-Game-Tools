/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Input
{
    using UnityEngine;

    /// <summary>
    /// This components draws a selection rectangle. It requires a specific setup so unless you feel you need to change anything, use the prefab that ships with Apex Path.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Selection Rectangle", 1041)]
    public class SelectionRectangleComponent : MonoBehaviour
    {
        /// <summary>
        /// Determines how much the mouse will need to be moved before the selection rectangle starts drawing.
        /// </summary>
        public float startDeltaThreshold = 3.0f;

        private Camera _selectionVisualCamera;
        private Transform _selectionVisual;

        private void Awake()
        {
            _selectionVisualCamera = this.GetComponentInChildren<Camera>();
            _selectionVisual = this.GetComponentInChildren<MeshRenderer>().transform;

            ToggleEnabled(false);
        }

        internal void StartSelect()
        {
            ToggleEnabled(true);
        }

        internal bool HasSelection(Vector3 startScreen, Vector3 endScreen)
        {
            if ((Mathf.Abs(startScreen.x - endScreen.x) < this.startDeltaThreshold) || (Mathf.Abs(startScreen.y - endScreen.y) < this.startDeltaThreshold))
            {
                return false;
            }

            DrawSelectionRect(startScreen, endScreen);

            return true;
        }

        internal void EndSelect()
        {
            ToggleEnabled(false);
        }

        private void ToggleEnabled(bool enabled)
        {
            _selectionVisualCamera.enabled = enabled;

            if (!enabled)
            {
                _selectionVisual.localScale = Vector3.zero;
            }
        }

        private void DrawSelectionRect(Vector3 startScreen, Vector3 endScreen)
        {
            var startWorld = _selectionVisualCamera.ScreenToWorldPoint(startScreen);
            var endWorld = _selectionVisualCamera.ScreenToWorldPoint(endScreen);

            var dx = endWorld.x - startWorld.x;
            var dy = endWorld.y - startWorld.y;

            _selectionVisual.position = new Vector3(
                startWorld.x + (dx / 2.0f),
                startWorld.y + (dy / 2.0f));

            _selectionVisual.localScale = new Vector3(Mathf.Abs(dx), Mathf.Abs(dy), 1.0f);
        }
    }
}
