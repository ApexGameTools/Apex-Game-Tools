/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System;
    using Serialization;
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(AIReferenceAttribute))]
    public class AIReferenceField : EditorFieldBase<Guid>
    {
        private string _aiName;
        private GUIContent _nameLabel = new GUIContent();

        public AIReferenceField(MemberData data, object owner)
            : base(data, owner)
        {
            UpdateName();
        }

        public override void RenderField(AIInspectorState state)
        {
            if (string.IsNullOrEmpty(_aiName))
            {
                _nameLabel.text = "None";
            }
            else
            {
                _nameLabel.text = _aiName;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60f;
            EditorGUILayout.LabelField(_label, _nameLabel);
            EditorGUIUtility.labelWidth = 0f;

            if (GUILayout.Button("...", EditorStyling.Skinned.fixedButton))
            {
                GUI.changed = false;

                var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                AISelectorWindow.Get(
                    screenPos,
                    (ai) =>
                    {
                        var newVal = (ai == null) ? Guid.Empty : new Guid(ai.aiId);
                        if (_curValue != newVal)
                        {
                            if (newVal == state.currentAIUI.ai.id)
                            {
                                EditorUtility.DisplayDialog("Invalid AI", "You cannot execute an AI from within itself.", "OK");
                                return;
                            }

                            _aiName = (ai == null) ? null : ai.name;
                            UpdateValue(newVal, state);
                        }
                    });
            }

            EditorGUILayout.EndHorizontal();
        }

        private void UpdateName()
        {
            var ai = StoredAIs.GetById(_curValue.ToString());
            if (ai != null)
            {
                _aiName = ai.name;
            }
        }
    }
}
