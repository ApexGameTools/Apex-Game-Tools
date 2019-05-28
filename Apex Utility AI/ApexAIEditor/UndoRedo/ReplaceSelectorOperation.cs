/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using UnityEngine;

    internal sealed class ReplaceSelectorOperation : AIUIOperation, IUndoRedo
    {
        private SelectorView _target;
        private Selector _oldValue;
        private Selector _newValue;

        internal ReplaceSelectorOperation(AIUI ui, Selector oldValue, Selector newValue, SelectorView target)
            : base(ui)
        {
            _target = target;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        void IUndoRedo.Do()
        {
            _ui.ReplaceSelector(_target, _newValue, false);
        }

        void IUndoRedo.Undo()
        {
            _ui.ReplaceSelector(_target, _oldValue, false);
        }
    }
}
