/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System.Collections.Generic;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;
    using Visualization;

    [CustomEditor(typeof(ContextVisualizerComponent), true)]
    public class ContextGizmoVisualizerComponentEditor : Editor
    {
        private static readonly GUIContent _relevantAILabel = new GUIContent("Relevant AI", "If the visualization should only take place for a certain AI, it can be selected here.");
        private static readonly GUIContent _relevantAIValue = new GUIContent();
        private static readonly GUIContent _modeLabel = new GUIContent("Mode", "Determines which contexts are visualized.\nIn Single mode it's only the currently selected GameObject's context.\nIn All mode it's all selected GameObjects.\nIn custom mode the selection is implemented specifically in the visualizer.");

        private ContextVisualizerComponent _target;
        private string _relevantAiName;
        private List<SerializedProperty> _props;

        public override void OnInspectorGUI()
        {
            EditorStyling.InitScaleAgnosticStyles();
            EditorGUILayout.Separator();
#if !UNITY_5 && !UNITY_2017
            if (_target.mode != SceneVisualizationMode.Custom)
            {
                EditorGUILayout.HelpBox("You need to have an Apex AI Editor window open and visibly docked, for selection changes to register.", MessageType.Info);
            }
#endif
            DrawAISelector();
            var mode = (SceneVisualizationMode)EditorGUILayout.EnumPopup(_modeLabel, _target.mode);
            if (mode != _target.mode)
            {
                _target.mode = mode;
                EditorUtility.SetDirty(_target);
            }

            this.serializedObject.Update();

            EditorGUILayout.Separator();
            foreach (var p in _props)
            {
                EditorGUILayout.PropertyField(p);
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawAISelector()
        {
            EditorGUILayout.BeginHorizontal();
            _relevantAIValue.text = _relevantAiName;
            EditorGUILayout.LabelField(_relevantAILabel, _relevantAIValue);

            if (GUILayout.Button(SharedStyles.changeSelectionTooltip, SharedStyles.BuiltIn.changeButtonSmall))
            {
                GUI.changed = false;

                var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                var win = AISelectorWindow.Get(
                            screenPos,
                            (ai) =>
                            {
                                if (ai == null)
                                {
                                    if (!string.IsNullOrEmpty(_target.relevantAIId))
                                    {
                                        _relevantAiName = "All";
                                        _target.relevantAIId = null;
                                        EditorUtility.SetDirty(_target);
                                    }
                                }
                                else if (_target.relevantAIId != ai.aiId)
                                {
                                    _relevantAiName = ai.name;
                                    _target.relevantAIId = ai.aiId;
                                    EditorUtility.SetDirty(_target);
                                }
                            },
                            true);

                var curSelectedId = _target.relevantAIId;
                win.Preselect((ai) => ai.aiId == curSelectedId);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            _props = this.GetProperties("relevantAIId", "mode");

            _target = this.target as ContextVisualizerComponent;
            if (string.IsNullOrEmpty(_target.relevantAIId))
            {
                _relevantAiName = "All";
                return;
            }

            var ai = StoredAIs.GetById(_target.relevantAIId);
            if (ai != null)
            {
                _relevantAiName = ai.name;
            }
            else
            {
                _relevantAiName = "?";
            }
        }
    }
}
