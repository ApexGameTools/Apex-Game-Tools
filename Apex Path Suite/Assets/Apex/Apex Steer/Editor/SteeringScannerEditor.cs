namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SteeringScanner), false), CanEditMultipleObjects]
    public class SteeringScannerEditor : Editor
    {
        private SerializedProperty _scanInterval;
        private SerializedProperty _scanRadius;
        private SerializedProperty _forecastDistance;
        private SerializedProperty _sortUnitsWithDistance;
        private SerializedProperty _filterAwayUnitsInSameGroup;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying && GUI.changed)
            {
                var t = this.target as ISupportRuntimeStateChange;
                if (t != null)
                {
                    t.ReevaluateState();
                }
            }

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_scanInterval);
            EditorGUILayout.PropertyField(_scanRadius);
            EditorGUILayout.PropertyField(_forecastDistance);

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_sortUnitsWithDistance);
            EditorGUILayout.PropertyField(_filterAwayUnitsInSameGroup);
            this.serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _scanInterval = this.serializedObject.FindProperty("scanInterval");
            _scanRadius = this.serializedObject.FindProperty("scanRadius");
            _forecastDistance = this.serializedObject.FindProperty("forecastDistance");
            _sortUnitsWithDistance = this.serializedObject.FindProperty("sortUnitsWithDistance");
            _filterAwayUnitsInSameGroup = this.serializedObject.FindProperty("filterAwayUnitsInSameGroup");
        }
    }
}