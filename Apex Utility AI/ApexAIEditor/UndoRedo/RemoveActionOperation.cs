/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using Apex.AI.Serialization;
    using UnityEngine;

    internal sealed class RemoveActionOperation : AIUIOperation, IUndoRedo
    {
        private ActionView _target;

        internal RemoveActionOperation(AIUI ui, ActionView target)
            : base(ui)
        {
            _target = target;
        }

        internal bool ConnectedTo(TopLevelView view)
        {
            var connector = _target as IConnectorActionView;
            if (connector == null)
            {
                return false;
            }

            return object.ReferenceEquals(connector.targetView, view);
        }

        void IUndoRedo.Do()
        {
            _ui.RemoveAction(_target, false);
        }

        void IUndoRedo.Undo()
        {
            var parent = _target.parent;
            parent.qualifier.action = _target.action;
            parent.actionView = _target;

            if (_target.isSelectable)
            {
                _ui.selectedView = _target;
            }
        }
    }
}
