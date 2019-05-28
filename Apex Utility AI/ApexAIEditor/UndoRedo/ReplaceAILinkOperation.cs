/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using UnityEngine;

    internal sealed class ReplaceAILinkOperation : AIUIOperation, IUndoRedo
    {
        private AILinkView _target;
        private Guid _oldValue;
        private Guid _newValue;

        internal ReplaceAILinkOperation(AIUI ui, Guid oldValue, Guid newValue, AILinkView target)
            : base(ui)
        {
            _target = target;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        void IUndoRedo.Do()
        {
            _ui.ChangeAILink(_target, _newValue, false);
        }

        void IUndoRedo.Undo()
        {
            _ui.ChangeAILink(_target, _oldValue, false);
        }
    }
}
