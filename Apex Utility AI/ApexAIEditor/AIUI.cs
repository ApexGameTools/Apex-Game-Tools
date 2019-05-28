/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.AI.Editor.UndoRedo;
    using Apex.AI.Serialization;
    using Apex.AI.Visualization;
    using Apex.Editor;
    using Apex.Serialization;
    using UnityEditor;
    using UnityEngine;

    internal class AIUI
    {
        private AIVersion _aiVersion;
        private IUtilityAI _ai;
        private IUtilityAI _visualizedAI;
        private AIStorage _aiStorage;
        private AIInspectorState _inspectorState;
        private UndoRedoManager _undoRedo;

        // The currently selected member in each hierarchy level
        private AILinkView _currentAILink;
        private SelectorView _currentSelector;
        private QualifierView _currentQualifier;
        private ActionView _currentAction;

        private List<TopLevelView> _selectedViews;

        internal AICanvas canvas;
        internal string name;

        private AIUI()
        {
            _aiVersion = AIVersion.FromVersion(this.GetType().Assembly.GetName().Version);
            _undoRedo = new UndoRedoManager(this);
            _selectedViews = new List<TopLevelView>(4);
        }

        internal AIInspectorState inspectorState
        {
            get
            {
                if (_inspectorState == null || _inspectorState.Equals(null))
                {
                    _inspectorState = ScriptableObject.CreateInstance<AIInspectorState>();
                    _inspectorState.currentAIUI = this;
                    _inspectorState.hideFlags = HideFlags.DontSave;
                }

                return _inspectorState;
            }
        }

        internal IUtilityAI ai
        {
            get { return _ai; }
        }

        internal UtilityAIVisualizer visualizedAI
        {
            get { return _visualizedAI as UtilityAIVisualizer; }
        }

        internal UndoRedoManager undoRedo
        {
            get { return _undoRedo; }
        }

        internal AILinkView currentAILink
        {
            get { return _currentAILink; }
        }

        internal SelectorView currentSelector
        {
            get { return _currentSelector; }
        }

        internal QualifierView currentQualifier
        {
            get { return _currentQualifier; }
        }

        internal ActionView currentAction
        {
            get { return _currentAction; }
        }

        internal bool isVisualizing
        {
            get { return _visualizedAI is UtilityAIVisualizer; }
        }

        internal bool isDirty
        {
            get;
            set;
        }

        internal Selector rootSelector
        {
            get
            {
                return _visualizedAI.rootSelector;
            }
        }

        internal IView selectedView
        {
            get
            {
                if (_currentAction != null)
                {
                    return _currentAction;
                }

                if (_currentQualifier != null)
                {
                    return _currentQualifier;
                }

                if (_currentSelector != null)
                {
                    return _currentSelector;
                }

                if (_currentAILink != null)
                {
                    return _currentAILink;
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    ResetSelections();
                    return;
                }
                else if (object.ReferenceEquals(value, this.selectedView))
                {
                    return;
                }

                var t = value.GetType();
                if (typeof(ActionView).IsAssignableFrom(t))
                {
                    var av = (ActionView)value;
                    Select(av.parent.parent, av.parent, av);
                }
                else if (t == typeof(QualifierView))
                {
                    var qv = (QualifierView)value;
                    Select(qv.parent, qv, null);
                }
                else if (t == typeof(SelectorView))
                {
                    var sv = (SelectorView)value;
                    Select(sv, null, null);
                }
                else if (t == typeof(AILinkView))
                {
                    var alv = (AILinkView)value;
                    Select(alv);
                }
            }
        }

        internal List<TopLevelView> selectedViews
        {
            get { return _selectedViews; }
        }

        internal static AIUI Create(string name)
        {
            var gui = new AIUI();
            gui.InitNew(name);
            gui.InitAI();

            return gui;
        }

        internal static AIUI Load(string aiId, bool refreshState)
        {
            var data = StoredAIs.GetById(aiId);
            if (data == null)
            {
                EditorUtility.DisplayDialog("Load Failed.", "The requested AI can no longer be found.", "Ok");
                return null;
            }

            return Load(data, refreshState);
        }

        internal static AIUI Load(AIStorage ai, bool refreshState)
        {
            var gui = new AIUI();
            gui.name = ai.name;

            if (!gui.LoadFrom(ai, refreshState))
            {
                return null;
            }

            return gui;
        }

        internal static bool VerifyCountMatch(int countA, int countB)
        {
            if (countA != countB)
            {
                EditorUtility.DisplayDialog("Load Error", "The AI could not be loaded for editing, due to a mismatch between editor data and AI data.", "Ok");
                return false;
            }

            return true;
        }

        internal void InitAI()
        {
            if (_ai == null)
            {
                _ai = _visualizedAI = new UtilityAI();
            }
        }

        internal void PingAsset()
        {
            if (_aiStorage == null)
            {
                return;
            }

            if (!UserSettings.instance.pingAsset)
            {
                return;
            }

            EditorGUIUtility.PingObject(_aiStorage);
        }

        internal void RefreshState()
        {
            this.inspectorState.Refresh();
        }

        internal void ResetSelections()
        {
            _selectedViews.Clear();
            ResetSingleSelection(true);
        }

        internal void MultiSelectViews(IEnumerable<TopLevelView> views)
        {
            _selectedViews = views.ToList();

            if (_selectedViews.Count == 1)
            {
                var v = _selectedViews[0];
                if (v is SelectorView)
                {
                    Select((SelectorView)v, null, null);
                }
                else if (v is AILinkView)
                {
                    Select((AILinkView)v);
                }

                return;
            }

            ResetSingleSelection(true);
        }

        internal void MultiSelectView(TopLevelView view)
        {
            if (!_selectedViews.Contains(view))
            {
                _selectedViews.Add(view);
            }
            else
            {
                _selectedViews.Remove(view);
            }

            if (_selectedViews.Count == 1)
            {
                view = _selectedViews[0];
                _currentSelector = view as SelectorView;
                _currentAILink = view as AILinkView;
            }
            else
            {
                ResetSingleSelection(false);
            }

            this.inspectorState.Refresh();
        }

        internal void Select(SelectorView sv, QualifierView qv, ActionView av)
        {
            _currentAILink = null;

            _currentSelector = sv;
            _selectedViews.Clear();
            if (sv != null)
            {
                _selectedViews.Add(sv);
            }

            _currentQualifier = qv;
            _currentAction = av;
            this.inspectorState.Refresh();
        }

        internal void Select(AILinkView alv)
        {
            _currentAILink = alv;
            _selectedViews.Clear();
            if (alv != null)
            {
                _selectedViews.Add(alv);
            }

            _currentSelector = null;
            _currentQualifier = null;
            _currentAction = null;

            this.inspectorState.Refresh();
        }

        internal bool RemoveView(TopLevelView view)
        {
            var sv = view as SelectorView;
            if (sv != null)
            {
                return RemoveSelector(sv);
            }

            var lv = view as AILinkView;
            if (lv != null)
            {
                RemoveAILink(lv);
                return true;
            }

            return false;
        }

        internal AILinkView AddAILink(Vector2 position, string aiId, bool recordUndo = true)
        {
            var aiGuid = new Guid(aiId);

            if (aiGuid == this._ai.id)
            {
                EditorUtility.DisplayDialog("Invalid Action", "You cannot add a link between an AI and itself.", "Ok");
                return null;
            }

            var viewRect = this.canvas.SnapToGrid(new Rect(position.x, position.y, 200f * this.canvas.zoom, 0f));
            var ail = new AILinkView(aiGuid, viewRect)
            {
                parent = this
            };

            this.canvas.views.Add(ail);
            Select(ail);

            this.isDirty = true;

            if (recordUndo)
            {
                _undoRedo.Do(new AddAILinkOperation(this, ail));
            }

            RefreshActiveEditor();
            return ail;
        }

        internal void ChangeAILink(AILinkView alv, Guid newAiId, bool recordUndo = true)
        {
            if (newAiId == this._ai.id)
            {
                EditorUtility.DisplayDialog("Invalid Action", "You cannot add a link between an AI and itself.", "Ok");
                return;
            }

            var refActions = GetReferencingActions(alv).ToArray();

            if (recordUndo)
            {
                _undoRedo.Do(new ReplaceAILinkOperation(this, alv.aiId, newAiId, alv));
            }

            alv.aiId = newAiId;

            foreach (var av in refActions)
            {
                var la = av.action as AILinkAction;
                if (la != null)
                {
                    la.aiId = newAiId;
                }
            }

            this.isDirty = true;
            RefreshActiveEditor();
        }

        internal void RemoveAILink(AILinkView alv, bool recordUndo = true)
        {
            if (alv == _currentAILink)
            {
                _currentAILink = null;
                this.inspectorState.Refresh();
            }

            RemoveTopLevelViewOperation undoOp = null;
            if (recordUndo)
            {
                undoOp = new RemoveAILinkOperation(this, alv);
                _undoRedo.Do(undoOp);
            }

            var refActions = GetReferencingActions(alv);
            foreach (var av in refActions)
            {
                if (recordUndo)
                {
                    undoOp.LogReferencingActionRemoval(av);
                }

                //References from composite actions must be treated differently
                var cav = av as CompositeActionView;
                if (cav != null)
                {
                    ResetConnection(cav, false);
                }
                else
                {
                    RemoveAction(av, false);
                }
            }

            this.canvas.views.Remove(alv);
            _selectedViews.Remove(alv);
            this.isDirty = true;
        }

        internal SelectorView FindSelector(Func<SelectorView, bool> predicate)
        {
            return (from v in this.canvas.views
                    let sv = v as SelectorView
                    where sv != null && predicate(sv)
                    select sv).FirstOrDefault();
        }

        internal AILinkView FindAILink(Func<AILinkView, bool> predicate)
        {
            return (from v in this.canvas.views
                    let alv = v as AILinkView
                    where alv != null && predicate(alv)
                    select alv).FirstOrDefault();
        }

        internal void SetRoot(Selector newRoot, bool recordUndo = true)
        {
            if (recordUndo)
            {
                _undoRedo.Do(new SetRootOperation(this, _ai.rootSelector, newRoot));
            }

            _ai.rootSelector = newRoot;

            this.inspectorState.Refresh();
            this.isDirty = true;
        }

        internal SelectorView AddSelector(Vector2 position, Type selectorType, bool recordUndo = true)
        {
            var viewRect = this.canvas.SnapToGrid(new Rect(position.x, position.y, 200f * this.canvas.zoom, 0f));
            var sv = SelectorView.Create(selectorType, this, viewRect);

            this.canvas.views.Add(sv);

            _ai.AddSelector(sv.selector);

            if (_ai.rootSelector == null)
            {
                _ai.rootSelector = sv.selector;
            }

            Select(sv, null, null);

            this.isDirty = true;

            if (recordUndo && !sv.isRoot)
            {
                _undoRedo.Do(new AddSelectorOperation(this, sv));
            }

            return sv;
        }

        internal bool RemoveSelector(SelectorView sv, bool recordUndo = true)
        {
            if (_ai.rootSelector == sv.selector)
            {
                EditorUtility.DisplayDialog("Invalid Action", "You cannot remove the root selector.\nTo remove the selected selector please assign another selector as the root first.", "Ok");
                return false;
            }

            if (sv == _currentSelector)
            {
                _currentSelector = null;
                this.inspectorState.Refresh();
            }

            RemoveTopLevelViewOperation undoOp = null;
            if (recordUndo)
            {
                undoOp = new RemoveSelectorOperation(this, sv);
                _undoRedo.Do(undoOp);
            }

            var refActions = GetReferencingActions(sv);
            foreach (var av in refActions)
            {
                if (recordUndo)
                {
                    undoOp.LogReferencingActionRemoval(av);
                }

                //References from composite actions must be treated differently
                var cav = av as CompositeActionView;
                if (cav != null)
                {
                    ResetConnection(cav, false);
                }
                else
                {
                    RemoveAction(av, false);
                }
            }

            this.canvas.views.Remove(sv);
            _ai.RemoveSelector(sv.selector);
            _selectedViews.Remove(sv);
            this.isDirty = true;

            return true;
        }

        internal void ReplaceSelector(SelectorView sv, Selector replacement, bool recordUndo = true)
        {
            //First get a hold of ref actions, we need to do this before we change the selector reference
            var refActions = GetReferencingActions(sv).ToArray();

            if (!_ai.ReplaceSelector(sv.selector, replacement))
            {
                return;
            }

            if (recordUndo)
            {
                _undoRedo.Do(new ReplaceSelectorOperation(this, sv.selector, replacement, sv));

                //We don't want to clone during undo/redo as the cloning is recorded
                TryClone(sv.selector, replacement);
            }

            sv.selector = replacement;

            foreach (var av in refActions)
            {
                var sa = av.action as SelectorAction;

                if (sa == null)
                {
                    var cav = av as CompositeActionView;
                    if (cav != null)
                    {
                        sa = cav.connectorAction as SelectorAction;
                    }
                }

                if (sa != null)
                {
                    sa.selector = replacement;
                }
            }

            //Need to refresh editor
            this.inspectorState.Refresh();

            this.isDirty = true;
        }

        internal QualifierView AddQualifier(Type qualifierType)
        {
            return AddQualifier(qualifierType, _currentSelector);
        }

        internal QualifierView AddQualifier(Type qualifierType, SelectorView parent, bool recordUndo = true)
        {
            if (qualifierType == null || parent == null)
            {
                return null;
            }

            var qv = QualifierView.Create(qualifierType);

            return AddQualifier(qv, parent);
        }

        internal QualifierView AddQualifier(QualifierView qv, SelectorView parent, bool recordUndo = true)
        {
            if (qv.parent != null && !object.ReferenceEquals(qv.parent, parent))
            {
                throw new InvalidOperationException("Cannot add a qualifier that already has a parent Selector.");
            }

            qv.parent = parent;
            parent.qualifierViews.Add(qv);
            parent.selector.qualifiers.Add(qv.qualifier);

            Select(parent, qv, null);

            this.isDirty = true;

            if (recordUndo)
            {
                _undoRedo.Do(new AddQualifierOperation(this, qv));
            }

            return qv;
        }

        internal void RemoveQualifier(QualifierView qv, bool recordUndo = true)
        {
            if (qv.isDefault)
            {
                return;
            }

            if (qv == _currentQualifier)
            {
                _currentQualifier = null;
                this.inspectorState.Refresh();
            }

            var parent = qv.parent;
            int qidx = parent.qualifierViews.IndexOf(qv);
            parent.qualifierViews.RemoveAt(qidx);
            parent.selector.qualifiers.RemoveAt(qidx);
            this.isDirty = true;

            if (recordUndo)
            {
                _undoRedo.Do(new RemoveQualifierOperation(this, qv, qidx));
            }
        }

        internal void ReplaceDefaultQualifier(QualifierView newDefault, SelectorView parent, bool recordUndo = true)
        {
            if (!newDefault.isDefault)
            {
                throw new ArgumentException("Qualifier must be a Default Qualifier.", "newDefault");
            }

            if (newDefault.parent != null && !object.ReferenceEquals(newDefault.parent, parent))
            {
                throw new InvalidOperationException("Cannot add a qualifier that already has a parent Selector.");
            }

            if (recordUndo)
            {
                _undoRedo.Do(new ReplaceDefaultQualifierOperation(this, parent.defaultQualifierView, newDefault, parent));
            }

            newDefault.parent = parent;
            parent.defaultQualifierView = newDefault;
            parent.selector.defaultQualifier = (IDefaultQualifier)newDefault.qualifier;

            Select(parent, newDefault, null);

            this.isDirty = true;
        }

        internal void ReplaceQualifier(QualifierView qv, IQualifier replacement, bool recordUndo = true)
        {
            var qualifiers = qv.parent.selector.qualifiers;
            var idx = qualifiers.IndexOf(qv.qualifier);
            if (idx < 0)
            {
                return;
            }

            if (recordUndo)
            {
                _undoRedo.Do(new ReplaceQualifierOperation(this, qv.qualifier, replacement, qv));

                //We don't want to clone during undo/redo as the cloning is recorded
                TryClone(qv.qualifier, replacement);
            }

            replacement.action = qv.qualifier.action;
            qualifiers[idx] = replacement;
            qv.qualifier = replacement;

            //Need to refresh editor
            this.inspectorState.Refresh();

            this.isDirty = true;
        }

        internal ActionView SetAction(Type actionType)
        {
            return SetAction(actionType, _currentQualifier);
        }

        internal ActionView SetAction(Type actionType, QualifierView parent)
        {
            if (actionType == null || parent == null)
            {
                return null;
            }

            if (parent.actionView != null)
            {
                var action = Activator.CreateInstance(actionType) as IAction;
                if (action != null)
                {
                    ReplaceAction(parent, action, true);
                    return parent.actionView;
                }
            }

            var av = ActionView.Create(actionType, parent);

            return SetAction(av, parent);
        }

        internal ActionView SetAction(ActionView av, QualifierView parent, bool recordUndo = true)
        {
            if (av.parent != null && !object.ReferenceEquals(av.parent, parent))
            {
                throw new InvalidOperationException("Cannot add an action that already has a parent Qualifier.");
            }

            var oldAction = parent.actionView;

            av.parent = parent;
            parent.qualifier.action = av.action;
            parent.actionView = av;

            if (av.isSelectable)
            {
                Select(parent.parent, parent, av);
            }

            this.isDirty = true;

            if (recordUndo)
            {
                if (oldAction != null)
                {
                    _undoRedo.Do(new SetActionOperation(this, oldAction, av));
                }
                else
                {
                    _undoRedo.Do(new SetActionOperation(this, av));
                }
            }

            return av;
        }

        internal void RemoveAction(ActionView av, bool recordUndo = true)
        {
            if (av == _currentAction)
            {
                _currentAction = null;
                this.inspectorState.Refresh();
            }

            var parent = av.parent;
            parent.qualifier.action = null;
            parent.actionView = null;
            this.isDirty = true;

            if (recordUndo)
            {
                _undoRedo.Do(new RemoveActionOperation(this, av));
            }
        }

        internal void ReplaceAction(QualifierView qv, IAction replacement, bool recordUndo = true)
        {
            var av = qv.actionView;
            if (av == null)
            {
                return;
            }

            if (recordUndo)
            {
                _undoRedo.Do(new ReplaceActionOperation(this, av.action, replacement, qv));

                //We don't want to clone during undo/redo as the cloning is recorded
                TryClone(av.action, replacement);
            }

            if (av is CompositeActionView && !(replacement is CompositeAction))
            {
                var tmp = new ActionView();
                tmp.parent = qv;
                tmp.name = av.name;
                tmp.description = av.description;
                av = tmp;
                qv.actionView = av;
            }
            else if (!(av is CompositeActionView) && replacement is CompositeAction)
            {
                var tmp = new CompositeActionView();
                tmp.parent = qv;
                tmp.name = av.name;
                tmp.description = av.description;
                av = tmp;
                qv.actionView = av;
            }
            else if (av is IConnectorActionView)
            {
                //For replacement of other connector views we need to reset to a normal ActionView.
                //Since it is not possible to replace an action with a connector, we don't handle the reverse.
                var tmp = new ActionView();
                tmp.parent = qv;
                tmp.name = av.name;
                tmp.description = av.description;
                av = tmp;
                qv.actionView = av;
            }

            av.action = replacement;
            qv.qualifier.action = replacement;

            //Need to refresh editor
            this.inspectorState.Refresh();

            this.isDirty = true;
        }

        internal bool RemoveSelected()
        {
            bool removed = false;

            if (_currentAction != null)
            {
                if (DisplayHelper.ConfirmDelete("Action"))
                {
                    RemoveAction(_currentAction);
                    removed = true;
                }
            }
            else if (_currentQualifier != null)
            {
                if (!_currentQualifier.isDefault && DisplayHelper.ConfirmDelete("Qualifier"))
                {
                    RemoveQualifier(_currentQualifier);
                    removed = true;
                }
            }
            else if (_selectedViews.Count > 0)
            {
                var selectedCount = _selectedViews.Count;
                if (DisplayHelper.ConfirmDelete("Selection"))
                {
                    using (_undoRedo.bulkOperation)
                    {
                        for (int i = selectedCount - 1; i >= 0; i--)
                        {
                            if (RemoveView(selectedViews[i]))
                            {
                                removed = true;
                            }
                        }
                    }
                }
            }

            if (removed)
            {
                this.isDirty = true;
                AIInspectorEditor.instance.Repaint();
                RefreshActiveEditor();
            }

            return removed;
        }

        internal bool Connect(QualifierView qv, TopLevelView targetView)
        {
            ActionView av = null;

            var sv = targetView as SelectorView;
            if (sv != null)
            {
                if (sv.selector.IsConnectedTo(qv.parent.selector))
                {
                    EditorUtility.DisplayDialog("Invalid Action", "Circular connections are not allowed.", "Ok");
                    return false;
                }

                //Composite actions may also act as selectors as the last step
                var cav = qv.actionView as CompositeActionView;
                if (cav != null)
                {
                    var ca = (CompositeAction)cav.action;
                    var newConnector = new SelectorAction(sv.selector);

                    _undoRedo.Do(new ConnectCompositeOperation(cav, ca.connectorAction, newConnector));
                    ca.connectorAction = newConnector;
                    cav.targetView = null;
                    return true;
                }

                av = new SelectorActionView
                {
                    targetSelector = sv,
                    action = new SelectorAction(sv.selector),
                    parent = qv
                };
            }

            var lv = targetView as AILinkView;
            if (lv != null)
            {
                var cav = qv.actionView as CompositeActionView;
                if (cav != null)
                {
                    var ca = (CompositeAction)cav.action;
                    var newConnector = new AILinkAction(lv.aiId);

                    _undoRedo.Do(new ConnectCompositeOperation(cav, ca.connectorAction, newConnector));
                    ca.connectorAction = newConnector;
                    cav.targetView = null;
                    return true;
                }

                av = new AILinkActionView
                {
                    targetAI = lv,
                    action = new AILinkAction(lv.aiId),
                    parent = qv
                };
            }

            if (av != null)
            {
                SetAction(av, qv);
                return true;
            }

            return false;
        }

        internal void ResetConnection(CompositeActionView cav, bool recordUndo = true)
        {
            var ca = (CompositeAction)cav.action;

            if (recordUndo)
            {
                _undoRedo.Do(new ConnectCompositeOperation(cav, ca.connectorAction, null));
            }

            ca.connectorAction = null;
            cav.targetView = null;
        }

        internal bool Zoom(Vector2 position, float zoom, ScaleSettings scaling)
        {
            if (this.canvas.Zoom(position, zoom, scaling))
            {
                _undoRedo.RegisterLayoutChange(ChangeTypes.Zoom);
                this.isDirty = true;
                return true;
            }

            return false;
        }

        internal void Save(string newName)
        {
            if (_ai == null || _ai.rootSelector == null)
            {
                return;
            }

            // new name is not null or empty when using 'Save As'
            if (!string.IsNullOrEmpty(newName))
            {
                this.name = StoredAIs.EnsureValidName(newName, _aiStorage);

                // If we are saving under a new name (as opposed to saving a new AI), we need to save copy of the current AI with new Ids.
                if (_aiStorage != null)
                {
                    _aiStorage = null;
                    _ai.RegenerateIds();
                }
            }

            bool saveNew = (_aiStorage == null);
            if (saveNew)
            {
                _aiStorage = AIStorage.Create(_ai.id.ToString(), this.name);
                StoredAIs.AIs.Add(_aiStorage);
                AssetPath.EnsurePath(AIGeneralSettings.instance.storagePath);
                var assetPath = AssetPath.Combine(AIGeneralSettings.instance.storagePath, this.name + ".asset");
                AssetDatabase.CreateAsset(_aiStorage, assetPath);
            }

            _aiStorage.version = _aiVersion.version;
            _aiStorage.configuration = SerializationMaster.Serialize(_ai);
            _aiStorage.editorConfiguration = GuiSerializer.Serialize(this.canvas);

            EditorUtility.SetDirty(_aiStorage);
            AssetDatabase.SaveAssets();
            this.isDirty = false;
            _undoRedo.SetSavePoint();

            if (saveNew && UserSettings.instance.autoGenerateNameMap)
            {
                AINameMapGenerator.WriteNameMapFile();
            }
        }

        internal bool Delete()
        {
            if (_aiStorage == null)
            {
                return true;
            }

            var path = AssetPath.Combine(AIGeneralSettings.instance.storagePath, string.Concat(_aiStorage.name, ".asset"));
            if (!AssetDatabase.DeleteAsset(path))
            {
                Debug.LogWarning(this.ToString() + " could not be deleted, path checked: " + path);
                return false;
            }

            if (UserSettings.instance.autoGenerateNameMap)
            {
                AINameMapGenerator.WriteNameMapFile();
            }

            return true;
        }

        internal void ShowVisualizedAI(UtilityAIVisualizer ai)
        {
            //The visualized ai is the same as the _ai instance in case no client is selected for visualization.
            if (ai == null)
            {
                if (object.ReferenceEquals(_visualizedAI, _ai))
                {
                    return;
                }

                _visualizedAI = _ai;
            }
            else
            {
                _visualizedAI = ai;
            }

            int i = 0;
            foreach (var sv in this.canvas.selectorViews)
            {
                sv.Reconnect(_visualizedAI[i++]);
            }
        }

        private static void TryClone(object current, object replacement)
        {
            var target = replacement as ICanClone;

            if (current != null && target != null)
            {
                target.CloneFrom(current);
            }
        }

        private void RefreshActiveEditor()
        {
            if (AIEditorWindow.activeInstance != null)
            {
                AIEditorWindow.activeInstance.Repaint();
            }
        }

        private void ResetSingleSelection(bool resfreshState)
        {
            _currentAILink = null;
            _currentSelector = null;
            _currentQualifier = null;
            _currentAction = null;

            if (resfreshState)
            {
                this.inspectorState.Refresh();
            }
        }

        private IEnumerable<ActionView> GetReferencingActions(SelectorView sv)
        {
            foreach (var selectorView in this.canvas.selectorViews)
            {
                if (selectorView == sv)
                {
                    // no need to check the one being removed
                    continue;
                }

                var qualifierViews = selectorView.qualifierViews;
                var qualifiersCount = qualifierViews.Count;
                for (int j = 0; j < qualifiersCount; j++)
                {
                    var qualifierView = qualifierViews[j];
                    var actionView = qualifierView.actionView as SelectorActionView;
                    var compActionView = qualifierView.actionView as CompositeActionView;
                    if (actionView != null)
                    {
                        var action = (SelectorAction)actionView.action;
                        if (object.ReferenceEquals(action.selector, sv.selector))
                        {
                            yield return actionView;
                        }
                    }
                    else if (compActionView != null)
                    {
                        var action = compActionView.connectorAction as SelectorAction;
                        if (action != null && object.ReferenceEquals(action.selector, sv.selector))
                        {
                            yield return compActionView;
                        }
                    }
                }

                var defaultQualifier = selectorView.defaultQualifierView;
                var selectorActionView = defaultQualifier.actionView as SelectorActionView;
                var compositeActionView = defaultQualifier.actionView as CompositeActionView;
                if (selectorActionView != null)
                {
                    var action = (SelectorAction)selectorActionView.action;
                    if (object.ReferenceEquals(action.selector, sv.selector))
                    {
                        yield return selectorActionView;
                    }
                }
                else if (compositeActionView != null)
                {
                    var action = compositeActionView.connectorAction as SelectorAction;
                    if (action != null && object.ReferenceEquals(action.selector, sv.selector))
                    {
                        yield return compositeActionView;
                    }
                }
            }
        }

        private IEnumerable<ActionView> GetReferencingActions(AILinkView alv)
        {
            foreach (var selectorView in this.canvas.selectorViews)
            {
                foreach (var qualifierView in selectorView.qualifierViews)
                {
                    var actionView = qualifierView.actionView as AILinkActionView;
                    var compActionView = qualifierView.actionView as CompositeActionView;
                    if (actionView != null)
                    {
                        var action = (AILinkAction)actionView.action;
                        if (action.aiId == alv.aiId)
                        {
                            yield return actionView;
                        }
                    }
                    else if (compActionView != null)
                    {
                        var action = compActionView.connectorAction as AILinkAction;
                        if (action != null && action.aiId == alv.aiId)
                        {
                            yield return compActionView;
                        }
                    }
                }

                var defaultQualifier = selectorView.defaultQualifierView;
                var defActionView = defaultQualifier.actionView as AILinkActionView;
                var compositeActionView = defaultQualifier.actionView as CompositeActionView;
                if (defActionView != null)
                {
                    var action = (AILinkAction)defActionView.action;
                    if (action.aiId == alv.aiId)
                    {
                        yield return defActionView;
                    }
                }
                else if (compositeActionView != null)
                {
                    var action = compositeActionView.connectorAction as AILinkAction;
                    if (action != null && action.aiId == alv.aiId)
                    {
                        yield return compositeActionView;
                    }
                }
            }
        }

        private bool LoadFrom(AIStorage data, bool refreshState)
        {
            _aiStorage = data;

            try
            {
                if (EditorApplication.isPlaying)
                {
                    _ai = _visualizedAI = AIManager.GetAI(new Guid(data.aiId));
                }
                else
                {
                    _ai = _visualizedAI = SerializationMaster.Deserialize<UtilityAI>(_aiStorage.configuration);
                }

                this.canvas = GuiSerializer.Deserialize(this, _aiStorage.editorConfiguration);
            }
            catch (Exception e)
            {
                if (EditorUtility.DisplayDialog("Load Error", "The AI could not be loaded, deserialization failed - see the console for details.\n\nDo you wish to open the AI repair tool?.", "Yes", "No"))
                {
                    RepairWindow.ShowWindow(data.name, data.aiId);
                }

                Debug.LogWarning("Failed to load AI: " + e.Message);
                return false;
            }

            var selectorViews = this.canvas.selectorViews.ToArray();
            int selectorCount = _ai.selectorCount;

            if (!VerifyCountMatch(selectorCount, selectorViews.Length))
            {
                return false;
            }

            for (int i = 0; i < selectorCount; i++)
            {
                if (!selectorViews[i].Reconnect(_ai[i]))
                {
                    return false;
                }
            }

            if (refreshState)
            {
                this.inspectorState.Refresh();
            }

            return true;
        }

        private void InitNew(string name)
        {
            this.canvas = new AICanvas();
            this.name = name;
        }
    }
}