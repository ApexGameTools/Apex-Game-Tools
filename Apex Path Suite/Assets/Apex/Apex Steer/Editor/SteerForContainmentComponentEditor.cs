namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;

    [CustomEditor(typeof(SteerForContainmentComponent), false), CanEditMultipleObjects]
    public class SteerForContainmentComponentEditor : SteeringComponentEditor
    {
        private SerializedProperty _bufferDistance;
        private SerializedProperty _drawGizmos;

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorUtilities.Section("Containment");

            EditorGUILayout.PropertyField(_bufferDistance);

            EditorGUI.indentLevel -= 1;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawGizmos);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _bufferDistance = this.serializedObject.FindProperty("bufferDistance");
            _drawGizmos = this.serializedObject.FindProperty("drawGizmos");
        }
    }
}