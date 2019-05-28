/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using UnityEngine;

    internal sealed class SetRootOperation : AIUIOperation, IUndoRedo
    {
        private Selector _oldValue;
        private Selector _newValue;

        internal SetRootOperation(AIUI ui, Selector oldValue, Selector newValue)
            : base(ui)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        void IUndoRedo.Do()
        {
            _ui.SetRoot(_newValue, false);
        }

        void IUndoRedo.Undo()
        {
            _ui.SetRoot(_oldValue, false);
        }
    }
}
