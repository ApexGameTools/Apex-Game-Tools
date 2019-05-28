/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEditor;
    using UnityEngine;

    internal sealed class AILinkLayout : ViewLayout
    {
        private const float _showAreaWidth = 40f;

        private float _containerHeight;
        private XRange _nameArea;
        private XRange _showArea;

        internal AILinkLayout(AILinkView linkView, float windowTop, ScaleSettings scaling)
            : base(linkView, windowTop, scaling)
        {
            var zoom = scaling.scale;
            _containerHeight = scaling.aiLinkBodyHeight;
            _nameArea = new XRange(_localViewRange.xMin + (10f * zoom), _localViewRange.width - ((_showAreaWidth + 10f) * zoom));
            _showArea = new XRange(_nameArea.xMax, _showAreaWidth * zoom);
        }

        internal Rect GetContainerAreaLocal()
        {
            return new Rect(0, _scaling.titleHeight, _viewRect.width, _containerHeight);
        }

        internal Rect GetNameAreaLocal()
        {
            return new Rect(_nameArea.xMin, _scaling.titleHeight, _nameArea.width, _containerHeight);
        }

        internal Rect GetShowAreaLocal()
        {
            return new Rect(_showArea.xMin, _scaling.titleHeight, _showArea.width, _containerHeight);
        }

        internal bool InShowArea(Vector2 mousePos)
        {
            var localPos = mousePos - _viewRect.position;
            if (localPos.y > this.titleHeight)
            {
                return _showArea.Contains(localPos.x);
            }

            return false;
        }
    }
}
