namespace Apex.Editor
{
    using Apex.Debugging;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PathVisualizer), false), CanEditMultipleObjects]
    public partial class PathVisualizerEditor : Editor
    {
        private SerializedProperty _routeColor;
        private SerializedProperty _waypointColor;
        private SerializedProperty _showSegmentMarkers;
        private SerializedProperty _drawAlways;

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawAlways);
            EditorGUILayout.PropertyField(_showSegmentMarkers);

            OnInspectorGUIPartial();

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_routeColor);
            EditorGUILayout.PropertyField(_waypointColor);
            this.serializedObject.ApplyModifiedProperties();
        }

        partial void OnInspectorGUIPartial();

        private void OnEnable()
        {
            _routeColor = this.serializedObject.FindProperty("routeColor");
            _waypointColor = this.serializedObject.FindProperty("waypointColor");
            _showSegmentMarkers = this.serializedObject.FindProperty("showSegmentMarkers");
            _drawAlways = this.serializedObject.FindProperty("drawAlways");

            OnEnablePartial();
        }

        partial void OnEnablePartial();
    }
}
