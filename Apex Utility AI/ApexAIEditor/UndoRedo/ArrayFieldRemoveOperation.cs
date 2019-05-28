/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.UndoRedo
{
    using System;
    using System.Collections;

    internal sealed class ArrayFieldRemoveOperation : IUndoRedo, IViewBoundOperation
    {
        private Array _arr;
        private int _index;
        private object _value;
        private Action<IList> _setter;

        internal ArrayFieldRemoveOperation(Array arr, Action<IList> setter, int index, object value)
        {
            _arr = arr;
            _index = index;
            _value = value;
            _setter = setter;
        }

        IView IViewBoundOperation.view
        {
            get;
            set;
        }

        void IUndoRedo.Do()
        {
            var itemType = _arr.GetType().GetElementType();
            var tmp = Array.CreateInstance(itemType, _arr.Length - 1);
            Array.Copy(_arr, 0, tmp, 0, _index);
            if (_index < tmp.Length)
            {
                Array.Copy(_arr, _index + 1, tmp, _index, tmp.Length - _index);
            }

            _arr = tmp;
            _setter(tmp);
        }

        void IUndoRedo.Undo()
        {
            var itemType = _arr.GetType().GetElementType();
            var tmp = Array.CreateInstance(itemType, _arr.Length + 1);
            Array.Copy(_arr, 0, tmp, 0, _index);
            if (_index < _arr.Length)
            {
                Array.Copy(_arr, _index, tmp, _index + 1, _arr.Length - _index);
            }

            tmp.SetValue(_value, _index);
            _arr = tmp;
            _setter(tmp);
        }
    }
}
