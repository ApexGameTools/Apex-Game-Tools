/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEngine;

    internal class ViewLayout
    {
        protected Rect _viewRect;
        protected XRange _localViewRange;
        protected ScaleSettings _scaling;

        private Rect _leftResizeArea;
        private Rect _rightResizeArea;

        public ViewLayout(TopLevelView view, float windowTop, ScaleSettings scaling)
        {
            _viewRect = view.viewArea;
            _scaling = scaling;

            _localViewRange = new XRange(scaling.selectorResizeMargin, _viewRect.xMax - _viewRect.xMin - (2f * scaling.selectorResizeMargin));

            var ymin = Mathf.Max(_viewRect.y, windowTop);
            _leftResizeArea = new Rect(_viewRect.xMin, ymin, scaling.selectorResizeMargin, _viewRect.height);
            _rightResizeArea = new Rect(_viewRect.xMax - scaling.selectorResizeMargin, ymin, scaling.selectorResizeMargin, _viewRect.height);
        }

        internal Rect viewRect
        {
            get { return _viewRect; }
        }

        internal float titleHeight
        {
            get { return _scaling.titleHeight; }
        }

        internal Rect leftResizeArea
        {
            get { return _leftResizeArea; }
        }

        internal Rect rightResizeArea
        {
            get { return _rightResizeArea; }
        }

        internal Rect GetIconArea(Rect parentArea)
        {
            float iconSize = 16f * _scaling.scale;
            return new Rect(parentArea.xMin + ((parentArea.width - iconSize) * 0.5f), parentArea.yMin + ((parentArea.height - iconSize) * 0.5f), iconSize, iconSize);
        }

        internal bool InTitleArea(Vector2 position)
        {
            return (position.y - _viewRect.y) <= _scaling.titleHeight;
        }

        internal bool InResizeArea(Vector2 position)
        {
            return _leftResizeArea.Contains(position) || _rightResizeArea.Contains(position);
        }
    }
}
