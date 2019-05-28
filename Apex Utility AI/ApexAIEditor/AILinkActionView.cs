/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using Apex.AI.Visualization;

    internal class AILinkActionView : ActionView, IConnectorActionView
    {
        private AILinkView _targetAI;

        internal AILinkView targetAI
        {
            get
            {
                if (_targetAI == null)
                {
                    var la = action as AILinkAction;
                    if (la == null)
                    {
                        var sav = action as ActionVisualizer;
                        if (sav != null)
                        {
                            la = sav.action as AILinkAction;
                        }
                    }

                    if (la != null)
                    {
                        var root = this.parent.parent.parent;
                        _targetAI = root.FindAILink(l => l.aiId == la.aiId);
                    }
                }

                return _targetAI;
            }

            set
            {
                _targetAI = value;
            }
        }

        internal override bool isSelectable
        {
            get { return false; }
        }

        public TopLevelView targetView
        {
            get { return this.targetAI; }
        }
    }
}