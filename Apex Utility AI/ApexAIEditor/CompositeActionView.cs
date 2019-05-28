/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using Apex.AI.Visualization;

    internal sealed class CompositeActionView : ActionView, IConnectorActionView
    {
        private TopLevelView _targetView;

        public TopLevelView targetView
        {
            get
            {
                if (_targetView == null)
                {
                    bool visualizing = false;
                    var ca = action as CompositeAction;
                    if (ca == null)
                    {
                        var sav = action as ActionVisualizer;
                        if (sav != null)
                        {
                            visualizing = true;
                            ca = sav.action as CompositeAction;
                        }
                    }

                    if (ca == null)
                    {
                        return null;
                    }

                    var sa = ca.connectorAction as SelectorAction;
                    if (sa != null)
                    {
                        var root = this.parent.parent.parent;
                        if (visualizing)
                        {
                            _targetView = root.FindSelector(s => object.ReferenceEquals(((SelectorVisualizer)s.selector).selector, sa.selector));
                        }
                        else
                        {
                            _targetView = root.FindSelector(s => object.ReferenceEquals(s.selector, sa.selector));
                        }
                    }

                    var la = ca.connectorAction as AILinkAction;
                    if (la != null)
                    {
                        var root = this.parent.parent.parent;
                        _targetView = root.FindAILink(l => l.aiId == la.aiId);
                    }
                }

                return _targetView;
            }

            internal set
            {
                _targetView = value;
            }
        }

        internal IAction connectorAction
        {
            get { return ((CompositeAction)this.action).connectorAction; }
        }
    }
}