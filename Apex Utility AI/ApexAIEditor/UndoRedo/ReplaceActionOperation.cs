/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using UnityEngine;

    internal sealed class ReplaceActionOperation : AIUIOperation, IUndoRedo
    {
        private QualifierView _target;
        private IAction _oldValue;
        private IAction _newValue;

        internal ReplaceActionOperation(AIUI ui, IAction oldValue, IAction newValue, QualifierView target)
            : base(ui)
        {
            _target = target;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        void IUndoRedo.Do()
        {
            _ui.ReplaceAction(_target, _newValue, false);
        }

        void IUndoRedo.Undo()
        {
            _ui.ReplaceAction(_target, _oldValue, false);
        }
    }
}
