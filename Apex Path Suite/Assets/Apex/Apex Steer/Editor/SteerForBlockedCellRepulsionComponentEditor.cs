namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SteerForBlockedCellRepulsionComponent), false), CanEditMultipleObjects]
    public class SteerForBlockedCellRepulsionComponentEditor : SteeringComponentEditor
    {
        private SerializedProperty _radiusMargin;
        private SerializedProperty _repulsionStrength;
        private SerializedProperty _drawGizmos;

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorUtilities.Section("Blocked Cell Repulsion");

            EditorGUILayout.PropertyField(_radiusMargin);
            EditorGUILayout.PropertyField(_repulsionStrength);

            EditorGUI.indentLevel -= 1;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawGizmos);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _radiusMargin = this.serializedObject.FindProperty("radiusMargin");
            _repulsionStrength = this.serializedObject.FindProperty("repulsionStrength");
            _drawGizmos = this.serializedObject.FindProperty("drawGizmos");
        }
    }
}
