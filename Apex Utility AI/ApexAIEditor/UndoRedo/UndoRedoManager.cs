/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System;

    internal sealed class UndoRedoManager
    {
        private readonly UndoRedoLog _log;
        private readonly BulkOp _bulkOperation;
        private int _layoutChangeCount;
        private int _functionalChangeCount;
        private ChangeTypes _layoutChanges;
        private AIUI _ui;

        internal UndoRedoManager(AIUI ui)
        {
            _ui = ui;
            _log = new UndoRedoLog(UserSettings.instance.maxUndo);
            _bulkOperation = new BulkOp(this);
        }

        internal bool canUndo
        {
            get
            {
                return _log.canUndo;
            }
        }

        internal bool canRedo
        {
            get
            {
                return _log.canRedo;
            }
        }

        internal bool isBulkOperation
        {
            get;
            private set;
        }

        internal IDisposable bulkOperation
        {
            get
            {
                this.isBulkOperation = true;
                return _bulkOperation;
            }
        }

        internal int layoutChangeCount
        {
            get { return _layoutChangeCount; }
        }

        internal int functionalChangeCount
        {
            get { return _functionalChangeCount; }
        }

        internal void SetSavePoint()
        {
            _layoutChangeCount = _functionalChangeCount = 0;
            _layoutChanges = ChangeTypes.None;
            _log.SetSavePoint();
        }

        internal void RegisterLayoutChange(ChangeTypes type)
        {
            if ((_layoutChanges & type) == 0)
            {
                _layoutChangeCount++;
                _layoutChanges |= type;
            }

            _log.InvalidateSavePoint();
        }

        internal void Do(IUndoRedo operation)
        {
            var viewBound = operation as IViewBoundOperation;
            if (viewBound != null)
            {
                viewBound.view = _ui.selectedView;
            }

            var last = (this.isBulkOperation ? _bulkOperation.lastOperation : _log.lastEntry) as IMergableOperation;
            if (last != null && last.TryMergeWith(operation, this.isBulkOperation))
            {
                return;
            }

            if (this.isBulkOperation)
            {
                _bulkOperation.lastOperation = operation;
            }

            _log.Register(operation);
            _functionalChangeCount++;
        }

        internal void Undo()
        {
            if (!_log.canUndo)
            {
                return;
            }

            var entry = _log.NextUndo();

            var viewBound = entry as IViewBoundOperation;
            if (viewBound != null)
            {
                viewBound.view.Select();
            }

            entry.Undo();

            _functionalChangeCount += _log.savePointRelatedDelta;
            _ui.isDirty = !_log.isSavePoint;

            _ui.RefreshState();
        }

        internal void Redo()
        {
            if (!_log.canRedo)
            {
                return;
            }

            var entry = _log.NextRedo();

            var viewBound = entry as IViewBoundOperation;
            if (viewBound != null)
            {
                viewBound.view.Select();
            }

            entry.Do();

            _functionalChangeCount += _log.savePointRelatedDelta;
            _ui.isDirty = !_log.isSavePoint;

            _ui.RefreshState();
        }

        private class BulkOp : IDisposable
        {
            private UndoRedoManager _parent;
            internal IUndoRedo lastOperation;

            internal BulkOp(UndoRedoManager parent)
            {
                _parent = parent;
            }

            public void Dispose()
            {
                _parent.isBulkOperation = false;
                lastOperation = null;
            }
        }
    }
}
