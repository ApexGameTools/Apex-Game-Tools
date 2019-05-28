namespace Apex.Editor
{
    using Apex.PathFinding;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PathServiceComponent), false)]
    public class PathServiceComponentEditor : Editor
    {
        private SerializedProperty _engineType;
        private SerializedProperty _moveCost;
        private SerializedProperty _initialHeapSize;
        private SerializedProperty _runAsync;
        private SerializedProperty _useThreadPoolForAsyncOperations;
        private SerializedProperty _maxMillisecondsPerFrame;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_engineType);
            EditorGUILayout.PropertyField(_moveCost);
            EditorGUILayout.PropertyField(_initialHeapSize);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_runAsync);
            if (_runAsync.boolValue)
            {
                EditorGUILayout.PropertyField(_useThreadPoolForAsyncOperations);
            }
            else
            {
                EditorGUILayout.PropertyField(_maxMillisecondsPerFrame);
            }

            this.serializedObject.ApplyModifiedProperties();

            GUI.enabled = true;
        }

        private void OnEnable()
        {
            _engineType = this.serializedObject.FindProperty("engineType");
            _moveCost = this.serializedObject.FindProperty("moveCost");
            _initialHeapSize = this.serializedObject.FindProperty("initialHeapSize");
            _runAsync = this.serializedObject.FindProperty("runAsync");
            _useThreadPoolForAsyncOperations = this.serializedObject.FindProperty("useThreadPoolForAsyncOperations");
            _maxMillisecondsPerFrame = this.serializedObject.FindProperty("maxMillisecondsPerFrame");
        }
    }
}
