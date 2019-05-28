/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System.Collections;

    internal sealed class ListFieldRemoveOperation : IUndoRedo, IViewBoundOperation
    {
        private IList _targetList;
        private int _index;
        private object _value;

        internal ListFieldRemoveOperation(IList targetList, int index, object value)
        {
            _targetList = targetList;
            _index = index;
            _value = value;
        }

        IView IViewBoundOperation.view
        {
            get;
            set;
        }

        void IUndoRedo.Do()
        {
            _targetList.RemoveAt(_index);
        }

        void IUndoRedo.Undo()
        {
            _targetList.Insert(_index, _value);
        }
    }
}
