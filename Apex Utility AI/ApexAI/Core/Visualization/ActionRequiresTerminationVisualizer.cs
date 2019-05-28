/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Visualization
{
    internal class ActionRequiresTerminationVisualizer : ActionVisualizer, IRequireTermination
    {
        private IRequireTermination _action;

        internal ActionRequiresTerminationVisualizer(IAction action, IQualifierVisualizer parent)
            : base(action, parent)
        {
            _action = (IRequireTermination)action;
        }

        public void Terminate(IAIContext context)
        {
            _action.Terminate(context);
        }
    }
}
