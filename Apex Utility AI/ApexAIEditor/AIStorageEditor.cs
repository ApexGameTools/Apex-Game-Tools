/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System.Text;
    using Apex.AI.Serialization;
    using Apex.Serialization;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(AIStorage), false)]
    public class AIStorageEditor : Editor
    {
        private bool _debug;
        private SerializedProperty _description;

        public override void OnInspectorGUI()
        {
            var ai = (AIStorage)this.target;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Apex AI: " + ai.name);

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_description);
            this.serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Open"))
            {
                AIEditorWindow.Open(ai.aiId);
            }

            EditorGUILayout.Separator();
            _debug = EditorGUILayout.ToggleLeft("Show debug options", _debug);
            if (_debug)
            {
                if (GUILayout.Button("Copy AI Configuration to clipboard"))
                {
                    try
                    {
                        var json = new StringBuilder();
                        var aiData = SerializationMaster.Deserialize(ai.configuration);
                        var guiData = SerializationMaster.Deserialize(ai.editorConfiguration);
                        json.AppendLine(SerializationMaster.Serialize(aiData, true));
                        json.AppendLine(SerializationMaster.Serialize(guiData, true));
                        EditorGUIUtility.systemCopyBuffer = json.ToString();
                    }
                    catch
                    {
                        EditorUtility.DisplayDialog("Error", "Copying failed, unable to read AI Configuration.", "OK");
                    }
                }
            }
        }

        private void OnEnable()
        {
            _description = this.serializedObject.FindProperty("description");
        }
    }
}
