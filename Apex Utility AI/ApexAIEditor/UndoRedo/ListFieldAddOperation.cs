/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class ListFieldAddOperation : IUndoRedo, IViewBoundOperation, IMergableOperation
    {
        private List<ListFieldAddOperation> _relatedAdds;
        private IList _targetList;
        private object _value;

        internal ListFieldAddOperation(IList targetList, object value)
        {
            _targetList = targetList;
            _value = value;
        }

        IView IViewBoundOperation.view
        {
            get;
            set;
        }

        void IUndoRedo.Do()
        {
            _targetList.Add(_value);

            if (_relatedAdds != null)
            {
                foreach (var a in _relatedAdds)
                {
                    _targetList.Add(a._value);
                }
            }
        }

        void IUndoRedo.Undo()
        {
            if (_relatedAdds != null)
            {
                var count = _relatedAdds.Count;
                for (int i = 0; i < count; i++)
                {
                    _targetList.RemoveAt(_targetList.Count - 1);
                }
            }

            _targetList.RemoveAt(_targetList.Count - 1);
        }

        bool IMergableOperation.TryMergeWith(IUndoRedo other, bool isBulkOperation)
        {
            if (!isBulkOperation)
            {
                return false;
            }

            var otherLFA = other as ListFieldAddOperation;
            if (otherLFA != null)
            {
                if (_relatedAdds == null)
                {
                    _relatedAdds = new List<ListFieldAddOperation>(1);
                }

                _relatedAdds.Add(otherLFA);
                return true;
            }

            return false;
        }
    }
}
