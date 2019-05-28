/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    internal sealed class SetActionOperation : AIUIOperation, IUndoRedo
    {
        private ActionView _oldAction;
        private ActionView _newAction;

        internal SetActionOperation(AIUI ui, ActionView newAction)
            : base(ui)
        {
            _newAction = newAction;
        }

        internal SetActionOperation(AIUI ui, ActionView oldAction, ActionView newAction)
            : base(ui)
        {
            _oldAction = oldAction;
            _newAction = newAction;
        }

        void IUndoRedo.Do()
        {
            _ui.SetAction(_newAction, _newAction.parent, false);
        }

        void IUndoRedo.Undo()
        {
            if (_oldAction == null)
            {
                _ui.RemoveAction(_newAction, false);
            }
            else
            {
                _ui.SetAction(_oldAction, _oldAction.parent, false);
            }
        }
    }
}
