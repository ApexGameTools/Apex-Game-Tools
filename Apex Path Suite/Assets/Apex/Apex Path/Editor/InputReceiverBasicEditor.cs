namespace Apex.Editor
{
    using Apex.Input;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(InputReceiverBasic), false), CanEditMultipleObjects]
    public class InputReceiverBasicEditor : Editor
    {
        private SerializedProperty _rightClickSupported;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component is not meant for production use, it is simply an example\nYou should create your own input receiver.", MessageType.Warning);

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_rightClickSupported);
            this.serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _rightClickSupported = this.serializedObject.FindProperty("rightClickSupported");
        }
    }
}
