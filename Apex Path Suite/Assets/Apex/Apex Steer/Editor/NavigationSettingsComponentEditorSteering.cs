namespace Apex.Editor
{
    using UnityEditor;

    public partial class NavigationSettingsComponentEditor
    {
        private SerializedProperty _groupUpdateInterval;

        partial void Initialize()
        {
            _groupUpdateInterval = this.serializedObject.FindProperty("groupUpdateInterval");
        }

        partial void InspectorGUI()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_groupUpdateInterval);
        }
    }
}