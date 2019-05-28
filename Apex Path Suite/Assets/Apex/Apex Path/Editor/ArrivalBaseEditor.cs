/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using UnityEditor;
    using UnityEngine;

    public abstract class ArrivalBaseEditor : SteeringComponentEditor
    {
        private SerializedProperty _slowingDistance;
        private SerializedProperty _autoCalculateSlowingDistance;
        private SerializedProperty _arrivalDistance;
        private SerializedProperty _slowingAlgorithm;

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorUtilities.Section("Arrival");
            EditorGUILayout.PropertyField(_slowingAlgorithm);
            EditorGUILayout.PropertyField(_autoCalculateSlowingDistance);
            if (!_autoCalculateSlowingDistance.boolValue)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(_slowingDistance);
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.PropertyField(_arrivalDistance);

            EditorGUI.indentLevel -= 1; 
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _slowingDistance = this.serializedObject.FindProperty("slowingDistance");
            _autoCalculateSlowingDistance = this.serializedObject.FindProperty("autoCalculateSlowingDistance");
            _arrivalDistance = this.serializedObject.FindProperty("arrivalDistance");
            _slowingAlgorithm = this.serializedObject.FindProperty("slowingAlgorithm");
        }
    }
}