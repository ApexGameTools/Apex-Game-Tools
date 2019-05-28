/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using UnityEngine;

    internal sealed class ReplaceQualifierOperation : AIUIOperation, IUndoRedo
    {
        private QualifierView _target;
        private IQualifier _oldValue;
        private IQualifier _newValue;

        internal ReplaceQualifierOperation(AIUI ui, IQualifier oldValue, IQualifier newValue, QualifierView target)
            : base(ui)
        {
            _target = target;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        void IUndoRedo.Do()
        {
            _ui.ReplaceQualifier(_target, _newValue, false);
        }

        void IUndoRedo.Undo()
        {
            _ui.ReplaceQualifier(_target, _oldValue, false);
        }
    }
}
