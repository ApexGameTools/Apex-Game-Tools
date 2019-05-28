/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.UndoRedo
{
    using System.Collections.Generic;

    internal abstract class AddTopLevelViewOperation : AIUIOperation, IUndoRedo, IMergableOperation
    {
        private List<IUndoRedo> _relatedAdds;
        private List<ActionView> _relatedActions;
        private IUndoRedo _rootChange;

        internal AddTopLevelViewOperation(AIUI ui)
            : base(ui)
        {
        }

        protected List<IUndoRedo> relatedAdds
        {
            get
            {
                if (_relatedAdds == null)
                {
                    _relatedAdds = new List<IUndoRedo>(1);
                }

                return _relatedAdds;
            }
        }

        void IUndoRedo.Do()
        {
            DoRedo();

            if (_relatedAdds != null)
            {
                foreach (var a in _relatedAdds)
                {
                    a.Do();
                }
            }

            if (_rootChange != null)
            {
                _rootChange.Do();
            }

            if (_relatedActions != null)
            {
                foreach (var a in _relatedActions)
                {
                    var parent = a.parent;
                    parent.qualifier.action = a.action;
                    parent.actionView = a;
                }
            }
        }

        void IUndoRedo.Undo()
        {
            if (_relatedAdds != null)
            {
                //Since a bulk add can include connectors between added views, we have to register those before an undo (removal) so they can be reestablished on a redo.
                if (_relatedActions == null)
                {
                    _relatedActions = new List<ActionView>();
                }
                else
                {
                    _relatedActions.Clear();
                }

                //First get connector actions for the whole operation
                foreach (var a in _relatedAdds)
                {
                    var aso = a as AddSelectorOperation;
                    if (aso == null)
                    {
                        continue;
                    }

                    foreach (var q in aso.target.qualifierViews)
                    {
                        if (q.actionView is IConnectorActionView)
                        {
                            _relatedActions.Add(q.actionView);
                        }
                    }
                }

                //Undo related
                if (_rootChange != null)
                {
                    _rootChange.Undo();
                }

                foreach (var a in _relatedAdds)
                {
                    a.Undo();
                }
            }

            DoUndo();
        }

        protected abstract void DoUndo();

        protected abstract void DoRedo();

        bool IMergableOperation.TryMergeWith(IUndoRedo other, bool isBulkOperation)
        {
            if (!isBulkOperation)
            {
                return false;
            }

            var otherTLV = other as AddTopLevelViewOperation;
            if (otherTLV != null)
            {
                this.relatedAdds.Add(otherTLV);
                return true;
            }

            var setRoot = other as SetRootOperation;
            if (setRoot != null)
            {
                _rootChange = setRoot;
                return true;
            }

            return false;
        }
    }
}
