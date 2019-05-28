/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    internal sealed class RemoveAILinkOperation : RemoveTopLevelViewOperation
    {
        private AILinkView _target;

        internal RemoveAILinkOperation(AIUI ui, AILinkView target)
            : base(ui)
        {
            _target = target;
        }

        protected override void DoUndo()
        {
            _ui.canvas.views.Add(_target);
            _ui.Select(_target);
        }

        protected override void DoRedo()
        {
            _ui.RemoveAILink(_target, false);
        }
    }
}
