/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using Apex.AI.Visualization;

    internal class ActionView : IView
    {
        internal QualifierView parent;

        private IAction _action;
        private Type _actionType;

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

        internal string friendlyName
        {
            get
            {
                if (string.IsNullOrEmpty(this.name) && _actionType != null)
                {
                    return DisplayHelper.GetFriendlyName(_actionType);
                }

                return this.name;
            }
        }

        internal string friendlyDescription
        {
            get
            {
                if (string.IsNullOrEmpty(this.description))
                {
                    return "Right Click for Options";
                }

                return this.description;
            }
        }

        internal IAction action
        {
            get
            {
                return _action;
            }

            set
            {
                _action = value;

                var av = _action as ActionVisualizer;
                _actionType = av == null ? _action.GetType() : av.action.GetType();
            }
        }

        internal virtual bool isSelectable
        {
            get { return true; }
        }

        AIUI IView.parentUI
        {
            get { return this.parent.parent.parent; }
        }

        public override string ToString()
        {
            return this.friendlyName;
        }

        internal static ActionView Create(Type actionType, QualifierView parent)
        {
            if (!typeof(IAction).IsAssignableFrom(actionType))
            {
                throw new ArgumentException("The proposed type is not an Action.", "actionType");
            }
            else if (actionType == typeof(SelectorAction) || actionType == typeof(AILinkAction))
            {
                throw new ArgumentException("Connector type action cannot be added through this method.", "actionType");
            }

            if (actionType == typeof(CompositeAction))
            {
                return new CompositeActionView
                {
                    action = new CompositeAction(),
                    parent = parent
                };
            }

            return new ActionView
            {
                action = Activator.CreateInstance(actionType) as IAction,
                parent = parent
            };
        }
    }
}