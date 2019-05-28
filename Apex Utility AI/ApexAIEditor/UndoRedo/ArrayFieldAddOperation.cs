/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class ArrayFieldAddOperation : IUndoRedo, IViewBoundOperation, IMergableOperation
    {
        private List<ArrayFieldAddOperation> _relatedAdds;
        private Array _arr;
        private object _value;
        private Action<IList> _setter;

        internal ArrayFieldAddOperation(Array arr, Action<IList> setter, object value)
        {
            _arr = arr;
            _setter = setter;
            _value = value;
        }

        IView IViewBoundOperation.view
        {
            get;
            set;
        }

        void IUndoRedo.Do()
        {
            var itemType = _arr.GetType().GetElementType();
            var addCount = (_relatedAdds != null) ? _relatedAdds.Count + 1 : 1;
            var tmp = Array.CreateInstance(itemType, _arr.Length + addCount);

            Array.Copy(_arr, 0, tmp, 0, _arr.Length);

            var idx = _arr.Length;
            tmp.SetValue(_value, idx++);

            if (_relatedAdds != null)
            {
                for (int i = 0;  i < addCount - 1; i++)
                {
                    var val = _relatedAdds[i]._value;
                    tmp.SetValue(val, idx + i);
                }
            }

            _arr = tmp;
            _setter(tmp);
        }

        void IUndoRedo.Undo()
        {
            //Undo is simple since the array reference is one tat only holds the one item added by this operation.
            //Related adds are as such irrelevant as they are removed automatically when we shrink the array.
            var itemType = _arr.GetType().GetElementType();
            var tmp = Array.CreateInstance(itemType, _arr.Length - 1);

            Array.Copy(_arr, 0, tmp, 0, tmp.Length);

            _arr = tmp;
            _setter(tmp);
        }

        bool IMergableOperation.TryMergeWith(IUndoRedo other, bool isBulkOperation)
        {
            if (!isBulkOperation)
            {
                return false;
            }

            var otherLFA = other as ArrayFieldAddOperation;
            if (otherLFA != null)
            {
                if (_relatedAdds == null)
                {
                    _relatedAdds = new List<ArrayFieldAddOperation>(1);
                }

                _relatedAdds.Add(otherLFA);

                //There is no need for related adds to hold onto their references as they are not used
                otherLFA._arr = null;
                otherLFA._setter = null;
                return true;
            }

            return false;
        }
    }
}
