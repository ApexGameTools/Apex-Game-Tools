/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.UndoRedo
{
    internal sealed class ConnectCompositeOperation : IUndoRedo
    {
        private CompositeActionView _target;
        private IConnectorAction _oldConnectorAction;
        private IConnectorAction _newConnectorAction;

        internal ConnectCompositeOperation(CompositeActionView target, IConnectorAction oldConnectorAction, IConnectorAction newConnectorAction)
        {
            _target = target;
            _oldConnectorAction = oldConnectorAction;
            _newConnectorAction = newConnectorAction;
        }

        void IUndoRedo.Do()
        {
            ((CompositeAction)_target.action).connectorAction = _newConnectorAction;
            _target.targetView = null;
        }

        void IUndoRedo.Undo()
        {
            ((CompositeAction)_target.action).connectorAction = _oldConnectorAction;
            _target.targetView = null;
        }
    }
}
