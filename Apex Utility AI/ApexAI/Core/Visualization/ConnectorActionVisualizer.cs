/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    internal abstract class ConnectorActionVisualizer : ActionVisualizer, IConnectorAction
    {
        internal ConnectorActionVisualizer(IAction action, IQualifierVisualizer parent)
            : base(action, parent)
        {
        }

        internal override void Execute(IAIContext context, bool doCallback)
        {
            /* NOOP */
        }

        public abstract IAction Select(IAIContext context);
    }
}
