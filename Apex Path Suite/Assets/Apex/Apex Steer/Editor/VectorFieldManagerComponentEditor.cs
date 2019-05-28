namespace Apex.Editor
{
    using Apex.Steering.VectorFields;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(VectorFieldManagerComponent), false), CanEditMultipleObjects]
    public class VectorFieldManagerComponentEditor : Editor
    {
        private SerializedProperty _vectorFieldOptions;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_vectorFieldOptions);
            this.serializedObject.ApplyModifiedProperties();

            GUI.enabled = true;
        }

        private void OnEnable()
        {
            _vectorFieldOptions = this.serializedObject.FindProperty("vectorFieldOptions");
        }
    }
}