namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SteerForUnitAvoidanceComponent), false), CanEditMultipleObjects]
    public class SteerForUnitAvoidanceComponentEditor : SteeringComponentEditor
    {
        private SerializedProperty _radiusMargin;
        private SerializedProperty _accumulateAvoidVectors;
        private SerializedProperty _headOnCollisionAngle;
        private SerializedProperty _minimumAvoidVectorMagnitude;
        private SerializedProperty _drawGizmos;
        private SerializedProperty _ignoredUnits;
        private SerializedProperty _preventPassingInFront;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying && GUI.changed)
            {
                var t = this.target as ISupportRuntimeStateChange;
                if (t != null)
                {
                    t.ReevaluateState();
                }
            }
        }

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorUtilities.Section("Avoidance");
            EditorGUILayout.PropertyField(_ignoredUnits);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_radiusMargin);
            EditorGUILayout.PropertyField(_minimumAvoidVectorMagnitude);
            EditorGUILayout.PropertyField(_headOnCollisionAngle);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_accumulateAvoidVectors);
            EditorGUILayout.PropertyField(_preventPassingInFront);

            EditorGUI.indentLevel -= 1;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawGizmos);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _radiusMargin = this.serializedObject.FindProperty("radiusMargin");
            _accumulateAvoidVectors = this.serializedObject.FindProperty("accumulateAvoidVectors");
            _headOnCollisionAngle = this.serializedObject.FindProperty("headOnCollisionAngle");
            _minimumAvoidVectorMagnitude = this.serializedObject.FindProperty("minimumAvoidVectorMagnitude");
            _drawGizmos = this.serializedObject.FindProperty("drawGizmos");
            _ignoredUnits = this.serializedObject.FindProperty("_ignoredUnits");
            _preventPassingInFront = this.serializedObject.FindProperty("preventPassingInFront");
        }
    }
}