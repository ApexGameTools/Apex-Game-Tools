/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    internal class AICanvas
    {
        internal Vector2 offset;
        internal float zoom = 1f;

        internal List<TopLevelView> views = new List<TopLevelView>();

        internal IEnumerable<SelectorView> selectorViews
        {
            get
            {
                return from v in views
                       let sv = v as SelectorView
                       where sv != null
                       select sv;
            }
        }

        internal TopLevelView ViewAtPosition(Vector2 position)
        {
            //We do this in reverse render order to select those visually on top in case of overlaps.
            var count = views.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var sv = views[i];
                if (sv.viewArea.Contains(position))
                {
                    return sv;
                }
            }

            return null;
        }

        internal bool Zoom(Vector2 position, float newScale, ScaleSettings scaling)
        {
            var settings = UserSettings.instance;
            newScale = Mathf.Clamp(newScale, settings.zoomMin, settings.zoomMax);

            //Due to the need for rounding to whole numbers (as Unity cannot render properly otherwise) we have to adjust the scale
            newScale = Mathf.Round(settings.snapCellSize * newScale) / settings.snapCellSize;

            var deltaScale = this.zoom - newScale;
            if (deltaScale * deltaScale < 0.000001f)
            {
                return false;
            }

            var deltaPan = (position * (deltaScale / this.zoom));

            var count = views.Count;
            for (int i = 0; i < count; i++)
            {
                views[i].Scale(this.zoom, newScale);
                views[i].viewArea.position += deltaPan;
            }

            this.offset = (this.offset.Scale(this.zoom, newScale) + deltaPan).Round();

            scaling.UpdateScale(newScale);
            this.zoom = newScale;
            SnapAllToGrid();
            return true;
        }

        internal Vector2 SnapPositionToGrid(Vector2 value)
        {
            var size = Mathf.Round(UserSettings.instance.snapCellSize * this.zoom);
            value -= this.offset;
            value = new Vector2(
                Mathf.Round(value.x / size) * size,
                Mathf.Round(value.y / size) * size);

            return value + this.offset;
        }

        internal Vector2 SnapSizeToGrid(Vector2 value)
        {
            var size = Mathf.Round(UserSettings.instance.snapCellSize * this.zoom);

            return new Vector2(
                Mathf.Round(value.x / size) * size,
                value.y);
        }

        internal Rect SnapToGrid(Rect value)
        {
            return new Rect
            {
                position = SnapPositionToGrid(value.position),
                size = SnapSizeToGrid(value.size)
            };
        }

        internal int SnapAllToGrid()
        {
            var count = views.Count;
            for (int i = 0; i < count; i++)
            {
                views[i].viewArea = SnapToGrid(views[i].viewArea);
            }

            return count;
        }
    }
}