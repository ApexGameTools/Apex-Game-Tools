/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEditor;
    using UnityEngine;

    internal sealed class VisualizedEntityLayout
    {
        private const float _visualizedEntityWidth = 200f;
        private const float _visualizedEntityHeight = 30f;

        private float _windowTop;
        private Rect _area;
        private XRange _nameArea;
        private XRange _stickyArea;
        private XRange _resetArea;

        internal VisualizedEntityLayout(Rect windowRect, float windowTop)
        {
            _windowTop = windowTop - 1f;

            _area = new Rect((windowRect.width - _visualizedEntityWidth) * 0.5f, _windowTop, _visualizedEntityWidth, _visualizedEntityHeight);

            float contentStart = _area.x + 5f;
            _resetArea = new XRange(contentStart + (_visualizedEntityWidth - 25f), 20f);
            _stickyArea = new XRange(contentStart + (_visualizedEntityWidth - 45f), 20f);
            _nameArea = new XRange(contentStart, _area.width - 40f);
        }

        internal Rect containerArea
        {
            get { return _area; }
        }

        internal Rect nameArea
        {
            get { return new Rect(_nameArea.xMin, _windowTop, _nameArea.width, _visualizedEntityHeight); }
        }

        internal Rect stickyArea
        {
            get { return new Rect(_stickyArea.xMin, _windowTop, _stickyArea.width, _visualizedEntityHeight); }
        }

        internal Rect resetArea
        {
            get { return new Rect(_resetArea.xMin, _windowTop, _resetArea.width, _visualizedEntityHeight); }
        }

        internal AreaHit GetHitInfo(Vector2 mousePos)
        {
            if (!_area.Contains(mousePos))
            {
                return null;
            }

            return new AreaHit(this, mousePos.x);
        }

        internal class AreaHit
        {
            private float _xpos;
            private VisualizedEntityLayout _parent;

            public AreaHit(VisualizedEntityLayout parent, float xpos)
            {
                _xpos = xpos;
                _parent = parent;
            }

            internal bool inStickyArea
            {
                get { return _parent._stickyArea.Contains(_xpos); }
            }

            internal bool inResetArea
            {
                get { return _parent._resetArea.Contains(_xpos); }
            }

            internal bool inNameArea
            {
                get { return _parent._nameArea.Contains(_xpos); }
            }
        }
    }
}
