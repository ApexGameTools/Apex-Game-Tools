/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using UnityEditor;
    using UnityEngine;

    public static class PopupConstraints
    {
        private static readonly RectOffset _edgePadding = new RectOffset(5, 5, 20, 5);

        public static Rect GetValidPosition(Rect winRect, Vector2 screenPosition, EditorWindow host)
        {
            var size = winRect.size;
            var halfWidth = size.x * 0.5f;
            var halfHeight = size.y * 0.5f;

            var range = GetValidRange(size, host.position);

            return new Rect(
                Mathf.Clamp(screenPosition.x - halfWidth, range.xMin, range.xMax),
                Mathf.Clamp(screenPosition.y - halfHeight, range.yMin, range.yMax),
                size.x,
                size.y);
        }

        private static Rect GetValidRange(Vector2 selfSize, Rect hostSize)
        {
            var container = new Rect
            {
                xMin = hostSize.xMin + _edgePadding.left,
                xMax = hostSize.xMax - selfSize.x - _edgePadding.right,
                yMin = hostSize.yMin + _edgePadding.top,
                yMax = hostSize.yMax - selfSize.y - _edgePadding.bottom
            };

            if (container.width < 0f)
            {
                container.xMin = hostSize.xMin + container.width;
            }

            if (container.height < 0f)
            {
                container.yMin = hostSize.yMin + container.height;
            }

            return container;
        }
    }
}
