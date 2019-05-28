namespace Apex.Editor
{
    using UnityEditor;

    public partial class PathVisualizerEditor
    {
        private SerializedProperty _drawVectorField;

        partial void OnEnablePartial()
        {
            _drawVectorField = this.serializedObject.FindProperty("drawVectorField");
        }

        partial void OnInspectorGUIPartial()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawVectorField);
        }
    }
}