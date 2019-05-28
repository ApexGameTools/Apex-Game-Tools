/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System.Linq;

    internal sealed class AILinkActionVisualizer : ConnectorActionVisualizer
    {
        private UtilityAIVisualizer _linkedAI;

        internal AILinkActionVisualizer(AILinkAction action, IQualifierVisualizer parent)
            : base(action, parent)
        {
        }

        internal override void Init()
        {
            var ai = ((AILinkAction)this.action).linkedAI;
            if (ai != null)
            {
                //We only want a single representation of a linked AI, otherwise multiple connections to the same linked AI in the same AI will not work as intended.
                var existingLinked = parent.parent.parent.linkedAIs;
                _linkedAI = existingLinked.FirstOrDefault(lai => lai.id == ai.id);
                if (_linkedAI == null)
                {
                    _linkedAI = new UtilityAIVisualizer(ai);
                    existingLinked.Add(_linkedAI);
                }
            }
        }

        public override IAction Select(IAIContext context)
        {
            //Since this action does nothing in the current AI, we can call post execute right away. The actual execution will happen in the linked AI.
            this.parent.parent.parent.PostExecute();

            if (_linkedAI == null)
            {
                return null;
            }

            return _linkedAI.Select(context);
        }
    }
}
