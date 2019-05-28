/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using System.Reflection;
    using UnityEngine;

    internal sealed class ViewValueChangeOperation : IUndoRedo, IMergableOperation, IViewBoundOperation
    {
        private string _originalValue;
        private string _newValue;
        private TargetValue _target;

        internal ViewValueChangeOperation(string orginalValue, string newValue, TargetValue target)
        {
            _target = target;
            _originalValue = orginalValue;
            _newValue = newValue;
        }

        internal enum TargetValue
        {
            Name,
            Description
        }

        public IView view
        {
            get;
            set;
        }

        bool IMergableOperation.TryMergeWith(IUndoRedo other, bool isBulkOperation)
        {
            var otherVCO = other as ViewValueChangeOperation;
            if (otherVCO == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this.view, otherVCO.view) && (_target == otherVCO._target))
            {
                _newValue = otherVCO._newValue;
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

        private void SetValue(string value)
        {
            if (_target == TargetValue.Name)
            {
                this.view.name = value;
            }
            else
            {
                this.view.description = value;
            }
        }
    }
}
