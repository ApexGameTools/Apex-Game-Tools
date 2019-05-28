/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using UnityEngine;

    internal sealed class ReplaceDefaultQualifierOperation : AIUIOperation, IUndoRedo
    {
        private SelectorView _target;
        private QualifierView _oldValue;
        private QualifierView _newValue;

        internal ReplaceDefaultQualifierOperation(AIUI ui, QualifierView oldValue, QualifierView newValue, SelectorView target)
            : base(ui)
        {
            _target = target;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        void IUndoRedo.Do()
        {
            _ui.ReplaceDefaultQualifier(_newValue, _target, false);
        }

        void IUndoRedo.Undo()
        {
            _ui.ReplaceDefaultQualifier(_oldValue, _target, false);
        }
    }
}
