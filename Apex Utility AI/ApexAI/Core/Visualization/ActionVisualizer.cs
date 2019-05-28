/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Visualization
{
    internal class ActionVisualizer : IAction, IVisualizedObject
    {
        private IAction _action;
        private IQualifierVisualizer _parent;

        internal ActionVisualizer(IAction action, IQualifierVisualizer parent)
        {
            _action = action;
            _parent = parent;
        }

        internal IAction action
        {
            get { return _action; }
        }

        internal IQualifierVisualizer parent
        {
            get { return _parent; }
        }

        object IVisualizedObject.target
        {
            get { return _action; }
        }

        internal virtual void Init()
        {
            /* NOOP */
        }

        internal virtual void Execute(IAIContext context, bool doCallback)
        {
            _action.Execute(context);
            if (doCallback)
            {
                _parent.parent.parent.PostExecute();
            }

            ICustomVisualizer customVisualizer;
            if (VisualizationManager.TryGetVisualizerFor(_action.GetType(), out customVisualizer))
            {
                customVisualizer.EntityUpdate(_action, context, _parent.parent.parent.id);
            }
        }

        public void Execute(IAIContext context)
        {
            Execute(context, true);
        }
    }
}
