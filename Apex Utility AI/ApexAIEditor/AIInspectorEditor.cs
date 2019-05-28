namespace Apex.AI.Editor
{
    using System;
    using Apex.AI.Editor.UndoRedo;
    using Apex.AI.Serialization;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(AIInspectorState), false)]
    public class AIInspectorEditor : Editor
    {
        internal static AIInspectorEditor instance;

        private AIInspectorState _state;

        internal AIInspectorEditor()
        {
            instance = this;
        }

        public override void OnInspectorGUI()
        {
            _state = this.target as AIInspectorState;
            if (_state.currentAIUI == null)
            {
                Selection.activeObject = null;
                return;
            }

            EditorStyling.InitScaleAgnosticStyles();

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            if (_state.currentAction != null)
            {
                DrawActionUI(_state.currentAction);
            }
            else if (_state.currentQualifier != null)
            {
                DrawQualifierUI(_state.currentQualifier);
            }
            else if (_state.currentSelector != null)
            {
                DrawSelectorUI(_state.currentSelector);
            }
            else if (_state.currentAILink != null)
            {
                DrawLinkUI(_state.currentAILink);
            }
            else if (_state.selectedCount > 1)
            {
                EditorGUILayout.LabelField(string.Concat("-- ", _state.selectedCount.ToString(), " Apex AI Selectors in Selection --"));
            }
            else if (_state.currentAIUI != null)
            {
                DrawAIUI(_state.currentAIUI);
            }
            else
            {
                EditorGUILayout.LabelField("-- No Apex AI Selection --");
            }

            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                _state.MarkDirty();
                RepaintEditorWindow();
            }

            var evt = Event.current;

            if (evt.type == EventType.KeyUp && evt.shift && (evt.command || evt.control))
            {
                if (evt.keyCode == KeyCode.Z)
                {
                    // CTRL/CMD + Shift + Z = Undo
                    _state.currentAIUI.undoRedo.Undo();
                    GUIUtility.keyboardControl = 0;
                    RepaintEditorWindow();
                }
                else if (evt.keyCode == KeyCode.Y)
                {
                    // CTRL/CMD + Shift + Y = Redo
                    _state.currentAIUI.undoRedo.Redo();
                    GUIUtility.keyboardControl = 0;
                    RepaintEditorWindow();
                }
            }
        }

        private void RepaintEditorWindow()
        {
            var win = AIEditorWindow.activeInstance;
            if (win != null)
            {
                win.Repaint();
            }
        }

        private void DrawLinkUI(AILinkView linkView)
        {
            HeaderHandler headerRects;
            if (EditorApplication.isPlaying)
            {
                headerRects = HeaderHandler.GetHeaderRects(0);
            }
            else
            {
                headerRects = HeaderHandler.GetHeaderRects(2);

                if (GUI.Button(headerRects.Next, SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
                {
                    GUI.changed = false;
                    _state.currentAIUI.RemoveSelected();
                    return;
                }

                if (GUI.Button(headerRects.Next, EditorStyling.changeTypeTooltip, SharedStyles.BuiltIn.changeButtonSmall))
                {
                    ShowChangeLinkMenu(linkView);
                }
            }

            GUI.Label(headerRects.Next, "AI Link | APEX AI", EditorStyling.Skinned.inspectorTitle);

            EditorGUILayout.Separator();

            DrawViewSharedUI(linkView);
        }

        private void DrawSelectorUI(SelectorView selectorView)
        {
            var isRoot = selectorView.isRoot;

            HeaderHandler headerRects;
            if (EditorApplication.isPlaying)
            {
                headerRects = HeaderHandler.GetHeaderRects(0);
            }
            else
            {
                if (isRoot)
                {
                    headerRects = HeaderHandler.GetHeaderRects(1);
                }
                else
                {
                    headerRects = HeaderHandler.GetHeaderRects(3);

                    if (GUI.Button(headerRects.Next, SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
                    {
                        GUI.changed = false;
                        _state.currentAIUI.RemoveSelected();
                        return;
                    }

                    if (GUI.Button(headerRects.Next, EditorStyling.setRootTooltip, EditorStyling.Skinned.setRootButtonSmall))
                    {
                        _state.currentAIUI.SetRoot(selectorView.selector);
                    }
                }

                if (GUI.Button(headerRects.Next, EditorStyling.changeTypeTooltip, SharedStyles.BuiltIn.changeButtonSmall))
                {
                    ShowChangeTypeMenu(selectorView.selector, (newSelector) => _state.currentAIUI.ReplaceSelector(selectorView, newSelector));
                }
            }

            GUI.Label(headerRects.Next, string.Concat(_state.editedItem.name, (isRoot ? " (ROOT) " : string.Empty), " | APEX AI"), EditorStyling.Skinned.inspectorTitle);

            EditorGUILayout.Separator();

            DrawViewSharedUI(selectorView);

            EditorGUILayout.Separator();

            _state.editedItem.Render(_state);
        }

        private void DrawQualifierUI(QualifierView qualifierView)
        {
            HeaderHandler headerRects;
            if (EditorApplication.isPlaying || qualifierView.isDefault)
            {
                headerRects = HeaderHandler.GetHeaderRects(0);
            }
            else
            {
                headerRects = HeaderHandler.GetHeaderRects(2);

                if (GUI.Button(headerRects.Next, SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
                {
                    GUI.changed = false;
                    _state.currentAIUI.RemoveSelected();
                    return;
                }

                if (GUI.Button(headerRects.Next, EditorStyling.changeTypeTooltip, SharedStyles.BuiltIn.changeButtonSmall))
                {
                    ShowChangeTypeMenu(qualifierView.qualifier, (newQualifier) => _state.currentAIUI.ReplaceQualifier(qualifierView, newQualifier));
                }
            }

            if (qualifierView.isDefault)
            {
                GUI.Label(headerRects.Next, string.Concat(_state.editedItem.name, " | APEX AI"), EditorStyling.Skinned.inspectorTitle);
            }
            else
            {
                var cbd = qualifierView.qualifier;
                var isDisabled = !EditorGUI.ToggleLeft(headerRects.Next, string.Concat(_state.editedItem.name, " | APEX AI"), !cbd.isDisabled, EditorStyling.Skinned.inspectorTitle);
                if (isDisabled != cbd.isDisabled)
                {
                    cbd.isDisabled = isDisabled;
                    _state.currentAIUI.undoRedo.Do(new DisableOperation(cbd));
                }
            }

            EditorGUILayout.Separator();

            DrawViewSharedUI(qualifierView);

            EditorGUILayout.Separator();

            _state.editedItem.Render(_state);
        }

        private void DrawActionUI(ActionView actionView)
        {
            HeaderHandler headerRects;
            if (EditorApplication.isPlaying)
            {
                headerRects = HeaderHandler.GetHeaderRects(0);
            }
            else
            {
                headerRects = HeaderHandler.GetHeaderRects(2);

                if (GUI.Button(headerRects.Next, SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
                {
                    GUI.changed = false;
                    _state.currentAIUI.RemoveSelected();
                    return;
                }

                if (GUI.Button(headerRects.Next, EditorStyling.changeTypeTooltip, SharedStyles.BuiltIn.changeButtonSmall))
                {
                    ShowChangeTypeMenu(actionView.action, (newAction) => _state.currentAIUI.ReplaceAction(actionView.parent, newAction));
                }
            }

            GUI.Label(headerRects.Next, string.Concat(_state.editedItem.name, " | APEX AI"), EditorStyling.Skinned.inspectorTitle);

            EditorGUILayout.Separator();

            DrawViewSharedUI(actionView);

            EditorGUILayout.Separator();

            _state.editedItem.Render(_state);
        }

        private void DrawAIUI(AIUI aiui)
        {
            HeaderHandler headerRects;
            if (EditorApplication.isPlaying)
            {
                headerRects = HeaderHandler.GetHeaderRects(0);
            }
            else
            {
                headerRects = HeaderHandler.GetHeaderRects(1);

                if (GUI.Button(headerRects.Next, SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
                {
                    GUI.changed = false;
                    if (DisplayHelper.ConfirmDelete("AI", true))
                    {
                        AIEditorWindow.activeInstance.DeleteAI();
                        return;
                    }
                }
            }

            GUI.Label(headerRects.Next, string.Concat(_state.currentAIUI.name, " | APEX AI"), EditorStyling.Skinned.inspectorTitle);
        }

        private void DrawViewSharedUI(IView view)
        {
            EditorGUIUtility.labelWidth = 60f;
            var name = EditorGUILayout.TextField("Name", view.name);
            if (name != view.name)
            {
                _state.currentAIUI.undoRedo.Do(new ViewValueChangeOperation(view.name, name, ViewValueChangeOperation.TargetValue.Name));
                view.name = name;
            }

            EditorGUILayout.LabelField("Description");
            
            var description = EditorGUILayout.TextArea(view.description, EditorStyling.Skinned.wrappedTextArea, GUILayout.Height(60f));
            if (description != view.description)
            {
                _state.currentAIUI.undoRedo.Do(new ViewValueChangeOperation(view.description, description, ViewValueChangeOperation.TargetValue.Description));
                view.description = description;
            }

            EditorGUIUtility.labelWidth = 0f;
        }

        private void ShowChangeTypeMenu<T>(T current, Action<T> replacerAction)
        {
            Action<Type> cb = (selectedType) =>
            {
                var newInstance = (T)Activator.CreateInstance(selectedType);

                replacerAction(newInstance);
            };

            // We do not want the button click itself to count as a change.
            GUI.changed = false;

            var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            AIEntitySelectorWindow.Get<T>(screenPos, cb);
        }

        private void ShowChangeLinkMenu(AILinkView lv)
        {
            Action<AIStorage> cb = (ai) =>
            {
                lv.parent.ChangeAILink(lv, new Guid(ai.aiId));
            };

            // We do not want the button click itself to count as a change.
            GUI.changed = false;

            var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            AISelectorWindow.Get(screenPos, cb);
        }

        private class HeaderHandler
        {
            private static readonly HeaderHandler _instance = new HeaderHandler();
            private Rect[] _rects = new Rect[4];
            private int _next;

            internal Rect Next
            {
                get { return _rects[_next++]; }
            }

            internal static HeaderHandler GetHeaderRects(int numberOfButtons)
            {
                _instance.Init(numberOfButtons);
                return _instance;
            }

            private void Init(int numberOfButtons)
            {
                _next = 0;

                GUILayout.Label(GUIContent.none);
                var headerRect = GUILayoutUtility.GetLastRect();

                for (int i = 0; i < numberOfButtons; i++)
                {
                    _rects[i] = new Rect(headerRect.xMax - ((i + 1) * 20f), headerRect.y, 20f, headerRect.height);
                }

                _rects[numberOfButtons] = new Rect(headerRect.x, headerRect.y, headerRect.width - (numberOfButtons * 20f), headerRect.height);
            }
        }
    }
}