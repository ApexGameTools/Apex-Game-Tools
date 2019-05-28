/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    internal sealed class RemoveSelectorOperation : RemoveTopLevelViewOperation
    {
        private SelectorView _target;

        internal RemoveSelectorOperation(AIUI ui, SelectorView target)
            : base(ui)
        {
            _target = target;
        }

        protected override void DoUndo()
        {
            _ui.canvas.views.Add(_target);
            _ui.ai.AddSelector(_target.selector);
            _ui.Select(_target, null, null);
        }

        protected override void DoRedo()
        {
            _ui.RemoveSelector(_target, false);
        }
    }
}
