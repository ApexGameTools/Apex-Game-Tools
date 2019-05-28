/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class CompositeActionVisualizer : ConnectorActionVisualizer, ICompositeVisualizer, ICompositeAction, IRequireTermination
    {
        private List<ActionVisualizer> _actions;
        private ConnectorActionVisualizer _connectorAction;

        internal CompositeActionVisualizer(CompositeAction action, IQualifierVisualizer parent)
            : base(action, parent)
        {
            _actions = new List<ActionVisualizer>(action.actions.Count);
            foreach (var a in action.actions)
            {
                var ca = a as CompositeAction;
                if (ca != null)
                {
                    _actions.Add(new CompositeActionVisualizer(ca, parent));
                }
                else
                {
                    _actions.Add(new ActionVisualizer(a, parent));
                }
            }

            var sa = action.connectorAction as SelectorAction;
            var la = action.connectorAction as AILinkAction;
            if (sa != null)
            {
                _connectorAction = new SelectorActionVisualizer(sa, parent);
            }
            else if (la != null)
            {
                _connectorAction = new AILinkActionVisualizer(la, parent);
            }
        }

        IList ICompositeVisualizer.children
        {
            get { return _actions; }
        }

        bool ICompositeAction.isConnector
        {
            get { return _connectorAction != null; }
        }

        internal override void Init()
        {
            if (_connectorAction != null)
            {
                _connectorAction.Init();
            }
        }

        internal override void Execute(IAIContext context, bool doCallback)
        {
            var count = _actions.Count;
            for (int i = 0; i < count; i++)
            {
                _actions[i].Execute(context, false);
            }

            if (doCallback)
            {
                parent.parent.parent.PostExecute();
            }
        }

        public override IAction Select(IAIContext context)
        {
            if (_connectorAction == null)
            {
                return null;
            }

            return _connectorAction.Select(context);
        }

        void ICompositeVisualizer.Add(object item)
        {
            var a = item as IAction;
            var ca = item as CompositeAction;
            if (ca != null)
            {
                _actions.Add(new CompositeActionVisualizer(ca, parent));
            }
            else if (a != null)
            {
                _actions.Add(new ActionVisualizer(a, parent));
            }
        }

        public void Terminate(IAIContext context)
        {
            var ta = this.action as IRequireTermination;
            if (ta != null)
            {
                ta.Terminate(context);
            }
        }
    }
}
