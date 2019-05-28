namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SteerForSeparationComponent), false), CanEditMultipleObjects]
    public class SteerForSeparationComponentEditor : SteeringComponentEditor
    {
        private SerializedProperty _separationDistance;
        private SerializedProperty _minimumForceMagnitude;
        private SerializedProperty _separationStrength;
        private SerializedProperty _blockedNeighboursBehaviour;
        private SerializedProperty _maximumUnitsToConsider;
        private SerializedProperty _ignoredUnits;
        private SerializedProperty _drawGizmos;

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorUtilities.Section("Separation");
            EditorGUILayout.PropertyField(_ignoredUnits);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_maximumUnitsToConsider);
            EditorGUILayout.PropertyField(_separationStrength);
            EditorGUILayout.PropertyField(_separationDistance);
            EditorGUILayout.PropertyField(_minimumForceMagnitude);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_blockedNeighboursBehaviour);

            EditorGUI.indentLevel -= 1;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawGizmos);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _separationDistance = this.serializedObject.FindProperty("separationDistance");
            _minimumForceMagnitude = this.serializedObject.FindProperty("minimumForceMagnitude");
            _separationStrength = this.serializedObject.FindProperty("separationStrength");
            _blockedNeighboursBehaviour = this.serializedObject.FindProperty("blockedNeighboursBehaviour");
            _maximumUnitsToConsider = this.serializedObject.FindProperty("maximumUnitsToConsider");
            _ignoredUnits = this.serializedObject.FindProperty("_ignoredUnits");
            _drawGizmos = this.serializedObject.FindProperty("drawGizmos");
        }
    }
}