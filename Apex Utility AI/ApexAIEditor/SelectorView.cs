/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using Apex.AI.Visualization;
    using UnityEngine;

    internal sealed class SelectorView : TopLevelView
    {
        private Selector _selector;
        private Type _selectorType;

        internal List<QualifierView> qualifierViews = new List<QualifierView>();
        internal QualifierView defaultQualifierView;

        internal SelectorView()
        {
        }

        private SelectorView(Rect viewArea)
            : base(viewArea)
        {
        }

        internal string friendlyName
        {
            get
            {
                if (string.IsNullOrEmpty(this.name) && _selectorType != null)
                {
                    return DisplayHelper.GetFriendlyName(_selectorType);
                }

                return this.name;
            }
        }

        internal Selector selector
        {
            get
            {
                return _selector;
            }

            set
            {
                _selector = value;

                var sv = _selector as SelectorVisualizer;
                _selectorType = sv == null ? _selector.GetType() : sv.selector.GetType();
            }
        }

        internal bool isRoot
        {
            get { return object.ReferenceEquals(this.selector, parent.rootSelector); }
        }

        internal IEnumerable<QualifierView> AllQualifierViews
        {
            get
            {
                var qualifierViews = this.qualifierViews;
                var count = qualifierViews.Count;
                for (int i = 0; i < count; i++)
                {
                    yield return qualifierViews[i];
                }

                yield return this.defaultQualifierView;
            }
        }

        internal bool reorderableQualifiers
        {
            get { return this.qualifierViews.Count > 1; }
        }

        public override string ToString()
        {
            return this.friendlyName;
        }

        internal static SelectorView Create(Type selectorType, AIUI parent, Rect viewArea)
        {
            if (!selectorType.IsSubclassOf(typeof(Selector)))
            {
                throw new ArgumentException("The proposed type is not a Selector.", "selectorType");
            }

            var sv = new SelectorView(viewArea);
            sv.parent = parent;
            sv.selector = Activator.CreateInstance(selectorType) as Selector;
            sv.defaultQualifierView = new QualifierView
            {
                qualifier = sv.selector.defaultQualifier,
                parent = sv
            };

            return sv;
        }

        internal override void RecalcHeight(ScaleSettings scaling)
        {
            var h = scaling.titleHeight;
            var qcount = this.qualifierViews.Count;
            for (int i = 0; i < qcount; i++)
            {
                h += this.qualifierViews[i].GetHeight(scaling);
            }

            h += this.defaultQualifierView.GetHeight(scaling);

            viewArea.height = h;
        }

        internal Vector3 ConnectorAnchorOut(int qualifierIdx, ScaleSettings scaling)
        {
            var ypos = scaling.titleHeight;

            //qualifierIdx > count in case of default qualifier
            var idxThreshold = Math.Min(qualifierIdx, this.qualifierViews.Count);
            for (int i = 0; i < idxThreshold; i++)
            {
                ypos += this.qualifierViews[i].GetHeight(scaling);
            }

            ypos += (qualifierIdx < this.qualifierViews.Count) ? this.qualifierViews[qualifierIdx].GetHeight(scaling) : this.defaultQualifierView.GetHeight(scaling);

            return new Vector3(
                         this.viewArea.x + this.viewArea.width,
                         this.viewArea.y + ypos - (scaling.actionHeight * 0.5f),
                         0f);
        }

        internal bool Reconnect(Selector s)
        {
            this.selector = s;

            var defQv = this.defaultQualifierView;
            defQv.qualifier = s.defaultQualifier;
            if (defQv.actionView != null)
            {
                //If an action view is present bug the action is null, this means inconsistency between config end editor config
                //While this should not occur, we still handle it just in case.
                if (defQv.qualifier.action == null)
                {
                    defQv.actionView = null;
                }
                else
                {
                    defQv.actionView.action = defQv.qualifier.action;
                }
            }

            var qualifiers = s.qualifiers;
            int qualifierCount = qualifiers.Count;

            if (!AIUI.VerifyCountMatch(qualifierCount, this.qualifierViews.Count))
            {
                return false;
            }

            for (int j = 0; j < qualifierCount; j++)
            {
                var qv = this.qualifierViews[j];
                qv.Reconnect(qualifiers[j]);
            }

            return true;
        }

        internal void PruneBrokenConnections()
        {
            this.defaultQualifierView.PruneBrokenConnections();

            int qualifierCount = this.qualifierViews.Count;
            for (int j = 0; j < qualifierCount; j++)
            {
                this.qualifierViews[j].PruneBrokenConnections();
            }
        }
    }
}