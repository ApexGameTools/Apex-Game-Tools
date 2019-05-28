/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using Apex.AI.Visualization;

    internal class SelectorActionView : ActionView, IConnectorActionView
    {
        private SelectorView _targetSelector;

        internal SelectorView targetSelector
        {
            get
            {
                if (_targetSelector == null)
                {
                    bool visualizing = false;
                    var sa = action as SelectorAction;
                    if (sa == null)
                    {
                        var sav = action as ActionVisualizer;
                        if (sav != null)
                        {
                            visualizing = true;
                            sa = sav.action as SelectorAction;
                        }
                    }

                    if (sa != null)
                    {
                        var root = this.parent.parent.parent;
                        if (visualizing)
                        {
                            _targetSelector = root.FindSelector(s => object.ReferenceEquals(((SelectorVisualizer)s.selector).selector, sa.selector));
                        }
                        else
                        {
                            _targetSelector = root.FindSelector(s => object.ReferenceEquals(s.selector, sa.selector));
                        }
                    }
                }

                return _targetSelector;
            }

            set
            {
                _targetSelector = value;
            }
        }

        internal override bool isSelectable
        {
            get { return false; }
        }

        public TopLevelView targetView
        {
            get { return this.targetSelector; }
        }
    }
}