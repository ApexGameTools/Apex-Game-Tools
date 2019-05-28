/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;

    internal sealed class CustomEditorFieldOperation : IUndoRedo, IViewBoundOperation
    {
        private object _originalValue;
        private object _newValue;
        private Action<object> _setter;

        internal CustomEditorFieldOperation(object orginalValue, object newValue, Action<object> setter)
        {
            _setter = setter;
            _originalValue = orginalValue;
            _newValue = newValue;
        }

        IView IViewBoundOperation.view
        {
            get;
            set;
        }

        void IUndoRedo.Do()
        {
            _setter(_newValue);
        }

        void IUndoRedo.Undo()
        {
            _setter(_originalValue);
        }
    }
}
