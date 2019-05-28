/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using Apex.WorldGeometry;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(LayerMappingComponent), false), CanEditMultipleObjects]
    public class LayerMappingComponentEditor : Editor
    {
        private SerializedProperty _staticObstacleLayer;
        private SerializedProperty _terrainLayer;
        private SerializedProperty _unitLayer;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_staticObstacleLayer);
            EditorGUILayout.PropertyField(_terrainLayer);
            EditorGUILayout.PropertyField(_unitLayer);
            this.serializedObject.ApplyModifiedProperties();

            GUI.enabled = true;
        }

        private void OnEnable()
        {
            _staticObstacleLayer = this.serializedObject.FindProperty("staticObstacleLayer");
            _terrainLayer = this.serializedObject.FindProperty("terrainLayer");
            _unitLayer = this.serializedObject.FindProperty("unitLayer");
        }
    }
}
