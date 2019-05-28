/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System.Collections.Generic;

    internal abstract class RemoveTopLevelViewOperation : AIUIOperation, IUndoRedo, IMergableOperation
    {
        private List<IUndoRedo> _relatedRemovals;
        private List<IUndoRedo> _relatedActionRemovals;

        internal RemoveTopLevelViewOperation(AIUI ui)
            : base(ui)
        {
        }

        protected List<IUndoRedo> relatedRemovals
        {
            get
            {
                if (_relatedRemovals == null)
                {
                    _relatedRemovals = new List<IUndoRedo>(1);
                }

                return _relatedRemovals;
            }
        }

        internal void LogReferencingActionRemoval(ActionView av)
        {
            if (_relatedActionRemovals == null)
            {
                _relatedActionRemovals = new List<IUndoRedo>(1);
            }

            var cav = av as CompositeActionView;
            if (cav != null)
            {
                var ca = (CompositeAction)cav.action;
                _relatedActionRemovals.Add(new ConnectCompositeOperation(cav, ca.connectorAction, null));
            }
            else
            {
                _relatedActionRemovals.Add(new RemoveActionOperation(_ui, av));
            }
        }

        void IUndoRedo.Do()
        {
            if (_relatedActionRemovals != null)
            {
                foreach (var a in _relatedActionRemovals)
                {
                    a.Do();
                }
            }

            if (_relatedRemovals != null)
            {
                foreach (var a in _relatedRemovals)
                {
                    a.Do();
                }
            }

            DoRedo();
        }

        void IUndoRedo.Undo()
        {
            DoUndo();

            if (_relatedRemovals != null)
            {
                foreach (var a in _relatedRemovals)
                {
                    a.Undo();
                }
            }

            if (_relatedActionRemovals != null)
            {
                foreach (var a in _relatedActionRemovals)
                {
                    a.Undo();
                }
            }
        }

        protected abstract void DoUndo();

        protected abstract void DoRedo();

        bool IMergableOperation.TryMergeWith(IUndoRedo other, bool isBulkOperation)
        {
            if (!isBulkOperation)
            {
                return false;
            }

            var otherTLV = other as RemoveTopLevelViewOperation;
            if (otherTLV != null)
            {
                this.relatedRemovals.Add(otherTLV);
                return true;
            }

            return false;
        }
    }
}
