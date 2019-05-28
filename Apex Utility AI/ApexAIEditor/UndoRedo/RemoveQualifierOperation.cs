/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using UnityEngine;

    internal sealed class RemoveQualifierOperation : AIUIOperation, IUndoRedo
    {
        private QualifierView _target;
        private int _targetIdx;

        internal RemoveQualifierOperation(AIUI ui, QualifierView target, int targetIdx)
            : base(ui)
        {
            _target = target;
            _targetIdx = targetIdx;
        }

        void IUndoRedo.Do()
        {
            _ui.RemoveQualifier(_target, false);
        }

        void IUndoRedo.Undo()
        {
            var parent = _target.parent;
            _ui.AddQualifier(_target, parent, false);

            int fromIdx = parent.qualifierViews.Count - 1;
            parent.qualifierViews.Reorder(fromIdx, _targetIdx);
            parent.selector.qualifiers.Reorder(fromIdx, _targetIdx);
        }
    }
}
