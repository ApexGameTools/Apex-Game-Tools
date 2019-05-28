/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System.Collections;

    internal sealed class ReorderOperation : IUndoRedo, IViewBoundOperation
    {
        private IList[] _targetLists;
        private int _oldIdx;
        private int _newIdx;

        internal ReorderOperation(int oldIdx, int newIdx, params IList[] targetLists)
        {
            _targetLists = targetLists;
            _oldIdx = oldIdx;
            _newIdx = newIdx;
        }

        IView IViewBoundOperation.view
        {
            get;
            set;
        }

        void IUndoRedo.Do()
        {
            for (int i = 0; i < _targetLists.Length; i++)
            {
                _targetLists[i].Reorder(_oldIdx, _newIdx);
            }
        }

        void IUndoRedo.Undo()
        {
            for (int i = 0; i < _targetLists.Length; i++)
            {
                _targetLists[i].Reorder(_newIdx, _oldIdx);
            }
        }
    }
}
