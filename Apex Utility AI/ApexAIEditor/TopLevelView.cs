/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEngine;

    public abstract class TopLevelView : IView
    {
        internal Rect viewArea;
        internal AIUI parent;

        protected TopLevelView()
        {
        }

        protected TopLevelView(Rect viewArea)
        {
            this.viewArea = viewArea;
        }

        public string name
        {
            get;
            set;
        }

        public string description
        {
            get;
            set;
        }

        internal bool isSelected
        {
            get { return parent.selectedViews.Contains(this); }
        }

        AIUI IView.parentUI
        {
            get { return this.parent; }
        }

        internal Vector3 ConnectorAnchorIn(ScaleSettings scaling)
        {
            return new Vector3(this.viewArea.x, this.viewArea.y + (scaling.titleHeight * 0.5f), 0f);
        }

        internal void Scale(float oldScale, float newScale)
        {
            this.viewArea = this.viewArea.Scale(oldScale, newScale);
        }

        internal virtual void RecalcHeight(ScaleSettings scaling)
        {
            var h = scaling.titleHeight;

            viewArea.height = h;
        }
    }
}
