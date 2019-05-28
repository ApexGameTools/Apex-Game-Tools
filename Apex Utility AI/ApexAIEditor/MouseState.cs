/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using Apex.Editor;
    using UnityEngine;

    internal sealed class MouseState
    {
        internal bool isMouseUp;

        internal bool isMouseDown;

        internal bool isMouseDrag;

        internal bool isMouseWheel;

        internal bool isLeftButton;

        internal bool isRightButton;

        internal bool isMiddleButton;

        internal void Update(Event evt)
        {
            isRightButton = evt.button == MouseButton.right;
            isLeftButton = evt.button == MouseButton.left;
            isMiddleButton = evt.button == MouseButton.middle;

            isMouseDown = evt.type == EventType.MouseDown;
            isMouseUp = evt.type == EventType.MouseUp;
            isMouseDrag = evt.type == EventType.MouseDrag;
            isMouseWheel = evt.type == EventType.ScrollWheel;
        }
    }
}
