/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using Apex.Steering;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(NavigationSettingsComponent), false)]
    public partial class NavigationSettingsComponentEditor : Editor
    {
        private SerializedProperty _heightSampling;
        private SerializedProperty _heightMapDetail;
        private SerializedProperty _heightSamplingGranularity;
        private SerializedProperty _ledgeThreshold;
        private SerializedProperty _useGlobalHeightNavigationSettings;
        private SerializedProperty _unitsHeightNavigationCapability;
        private SerializedProperty _useClearance;
        private SerializedProperty _heightDiffThreshold;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();
            EditorUtilities.Section("Height Sampling");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_heightSampling);
            if (EditorGUI.EndChangeCheck())
            {
                DefaultHeightNavigatorEditor.EnsureValidProviders((HeightSamplingMode)_heightSampling.intValue);
            }

            if (_heightSampling.intValue == (int)HeightSamplingMode.HeightMap)
            {
                EditorGUILayout.PropertyField(_heightMapDetail);
            }

            bool isFlat = (_heightSampling.intValue == (int)HeightSamplingMode.NoHeightSampling);
            if (!isFlat)
            {
                EditorGUILayout.PropertyField(_heightSamplingGranularity);
                EditorGUILayout.PropertyField(_ledgeThreshold);

                EditorGUILayout.Separator();
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(_useGlobalHeightNavigationSettings);
                if (_useGlobalHeightNavigationSettings.boolValue)
                {
                    EditorGUILayout.PropertyField(_unitsHeightNavigationCapability, GUIContent.none, true);
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_useClearance);
            if (!isFlat && _useClearance.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_heightDiffThreshold);
                EditorGUI.indentLevel--;
            }

            InspectorGUI();

            this.serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                var t = this.target as NavigationSettingsComponent;
                t.Refresh();
            }

            GUI.enabled = true;
        }

        partial void Initialize();

        partial void InspectorGUI();

        private void OnEnable()
        {
            _heightSampling = this.serializedObject.FindProperty("heightSampling");
            _heightMapDetail = this.serializedObject.FindProperty("heightMapDetail");
            _heightSamplingGranularity = this.serializedObject.FindProperty("heightSamplingGranularity");
            _ledgeThreshold = this.serializedObject.FindProperty("ledgeThreshold");
            _useGlobalHeightNavigationSettings = this.serializedObject.FindProperty("useGlobalHeightNavigationSettings");
            _unitsHeightNavigationCapability = this.serializedObject.FindProperty("unitsHeightNavigationCapability");
            _useClearance = this.serializedObject.FindProperty("useClearance");
            _heightDiffThreshold = this.serializedObject.FindProperty("heightDiffThreshold");

            Initialize();
        }
    }
}