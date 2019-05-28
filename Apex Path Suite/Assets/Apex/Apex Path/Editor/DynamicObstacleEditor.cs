/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using Apex.WorldGeometry;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(DynamicObstacle), false), CanEditMultipleObjects]
    public partial class DynamicObstacleEditor : Editor
    {
        private SerializedProperty _exceptions;
        private SerializedProperty _updateMode;
        private SerializedProperty _velocityPredictionFactor;
        private SerializedProperty _resolveVelocityFromParent;
        private SerializedProperty _stopUpdatingIfStationary;
        private SerializedProperty _stationaryThresholdSeconds;
        private SerializedProperty _useGridObstacleSensitivity;
        private SerializedProperty _customSensitivity;
        private SerializedProperty _customUpdateInterval;
        private SerializedProperty _supportDynamicGrids;
        private SerializedProperty _causesReplanning;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;
            this.serializedObject.Update();
            EditorUtilities.Section("Obstruction");
            EditorGUILayout.PropertyField(_exceptions);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_updateMode);
            EditorGUILayout.PropertyField(_customUpdateInterval);

            ExtensionOnGUI();

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_useGridObstacleSensitivity);
            if (_useGridObstacleSensitivity.boolValue == false)
            {
                EditorGUILayout.PropertyField(_customSensitivity);
            }

            EditorUtilities.Section("Grid interaction");
            EditorGUILayout.PropertyField(_supportDynamicGrids);
            EditorGUILayout.PropertyField(_causesReplanning);

            EditorUtilities.Section("Velocity");

            EditorGUILayout.PropertyField(_velocityPredictionFactor);
            EditorGUILayout.PropertyField(_resolveVelocityFromParent);
            EditorGUILayout.PropertyField(_stopUpdatingIfStationary);
            if (_stopUpdatingIfStationary.boolValue == true)
            {
                EditorGUILayout.PropertyField(_stationaryThresholdSeconds);
            }

            this.serializedObject.ApplyModifiedProperties();
            GUI.enabled = true;
        }

        partial void ExtensionEnable();

        partial void ExtensionOnGUI();

        private void OnEnable()
        {
            _exceptions = this.serializedObject.FindProperty("_exceptionsMask");
            _updateMode = this.serializedObject.FindProperty("updateMode");
            _velocityPredictionFactor = this.serializedObject.FindProperty("velocityPredictionFactor");
            _resolveVelocityFromParent = this.serializedObject.FindProperty("resolveVelocityFromParent");
            _stopUpdatingIfStationary = this.serializedObject.FindProperty("stopUpdatingIfStationary");
            _stationaryThresholdSeconds = this.serializedObject.FindProperty("stationaryThresholdSeconds");
            _useGridObstacleSensitivity = this.serializedObject.FindProperty("useGridObstacleSensitivity");
            _customSensitivity = this.serializedObject.FindProperty("customSensitivity");
            _customUpdateInterval = this.serializedObject.FindProperty("customUpdateInterval");
            _supportDynamicGrids = this.serializedObject.FindProperty("supportDynamicGrids");
            _causesReplanning = this.serializedObject.FindProperty("causesReplanning");

            ExtensionEnable();
        }
    }
}
