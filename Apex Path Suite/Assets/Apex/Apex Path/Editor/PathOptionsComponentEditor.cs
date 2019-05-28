namespace Apex.Editor
{
    using Apex.PathFinding;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PathOptionsComponent), false), CanEditMultipleObjects]
    public class PathOptionsComponentEditor : Editor
    {
        private SerializedProperty _pathingPriority;
        private SerializedProperty _usePathSmoothing;
        private SerializedProperty _optimizeUnobstructedPaths;
        private SerializedProperty _allowCornerCutting;
        private SerializedProperty _preventDiagonalMoves;
        private SerializedProperty _navigateToNearestIfBlocked;
        private SerializedProperty _maxEscapeCellDistanceIfOriginBlocked;
        private SerializedProperty _nextNodeDistance;
        private SerializedProperty _requestNextWaypointDistance;
        private SerializedProperty _announceAllNodes;
        private SerializedProperty _replanMode;
        private SerializedProperty _replanInterval;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();

            EditorUtilities.Section("Path Finder Options");
            EditorGUILayout.PropertyField(_pathingPriority);
            EditorGUILayout.PropertyField(_usePathSmoothing);
            EditorGUILayout.PropertyField(_optimizeUnobstructedPaths);
            EditorGUILayout.PropertyField(_allowCornerCutting);
            EditorGUILayout.PropertyField(_preventDiagonalMoves);
            EditorGUILayout.PropertyField(_navigateToNearestIfBlocked);
            EditorGUILayout.PropertyField(_maxEscapeCellDistanceIfOriginBlocked);

            EditorUtilities.Section("Path Following");
            EditorGUILayout.PropertyField(_nextNodeDistance);
            EditorGUILayout.PropertyField(_requestNextWaypointDistance);
            EditorGUILayout.PropertyField(_announceAllNodes);

            EditorUtilities.Section("Replanning");
            EditorGUILayout.PropertyField(_replanMode);
            EditorGUILayout.PropertyField(_replanInterval);

            this.serializedObject.ApplyModifiedProperties();

            GUI.enabled = true;
        }

        private void OnEnable()
        {
            _pathingPriority = this.serializedObject.FindProperty("_pathingPriority");
            _usePathSmoothing = this.serializedObject.FindProperty("_usePathSmoothing");
            _optimizeUnobstructedPaths = this.serializedObject.FindProperty("_optimizeUnobstructedPaths");
            _allowCornerCutting = this.serializedObject.FindProperty("_allowCornerCutting");
            _preventDiagonalMoves = this.serializedObject.FindProperty("_preventDiagonalMoves");
            _navigateToNearestIfBlocked = this.serializedObject.FindProperty("_navigateToNearestIfBlocked");
            _maxEscapeCellDistanceIfOriginBlocked = this.serializedObject.FindProperty("_maxEscapeCellDistanceIfOriginBlocked");
            _nextNodeDistance = this.serializedObject.FindProperty("_nextNodeDistance");
            _requestNextWaypointDistance = this.serializedObject.FindProperty("_requestNextWaypointDistance");
            _announceAllNodes = this.serializedObject.FindProperty("_announceAllNodes");
            _replanMode = this.serializedObject.FindProperty("_replanMode");
            _replanInterval = this.serializedObject.FindProperty("_replanInterval");
        }
    }
}
