/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using System.Reflection;
    using UnityEngine;

    internal sealed class EditorFieldOperation<T> : IUndoRedo, IMergableOperation, IViewBoundOperation
    {
        private T _originalValue;
        private T _newValue;
        private object _owner;
        private MemberInfo _member;

        internal EditorFieldOperation(object owner, MemberInfo member, T orginalValue, T newValue)
        {
            _owner = owner;
            _member = member;
            _originalValue = orginalValue;
            _newValue = newValue;
        }

        IView IViewBoundOperation.view
        {
            get;
            set;
        }

        bool IMergableOperation.TryMergeWith(IUndoRedo other, bool isBulkOperation)
        {
            var otherEFO = other as EditorFieldOperation<T>;
            if (otherEFO == null)
            {
                return false;
            }

            if (object.ReferenceEquals(_owner, otherEFO._owner) && object.ReferenceEquals(_member, otherEFO._member))
            {
                _newValue = otherEFO._newValue;
                return true;
            }

            return false;
        }

        void IUndoRedo.Do()
        {
            SetValue(_newValue);
        }

        void IUndoRedo.Undo()
        {
            SetValue(_originalValue);
        }

        private void SetValue(T value)
        {
            var prop = _member as PropertyInfo;
            if (prop != null)
            {
                prop.SetValue(_owner, value, null);
            }
            else
            {
                var field = _member as FieldInfo;
                if (field != null)
                {
                    field.SetValue(_owner, value);
                }
            }
        }
    }
}
