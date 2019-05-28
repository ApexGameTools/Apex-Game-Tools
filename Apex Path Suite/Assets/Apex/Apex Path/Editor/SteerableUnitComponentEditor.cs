/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using Apex.Steering;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SteerableUnitComponent), false), CanEditMultipleObjects]
    public class SteerableUnitComponentEditor : Editor
    {
        private SerializedProperty _stopIfStuckForSeconds;
        private SerializedProperty _stopTimeFrame;
        private SerializedProperty _averageActualVelocity;

        public override void OnInspectorGUI()
        {
            var c = this.target as SteerableUnitComponent;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Current Speed", c.speed.ToString());
            EditorGUILayout.LabelField("Current Velocity", c.velocity.ToString());
            EditorGUILayout.LabelField("Actual Velocity", c.actualVelocity.ToString());

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_averageActualVelocity);
            EditorGUILayout.PropertyField(_stopIfStuckForSeconds);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_stopTimeFrame);
            this.serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _stopIfStuckForSeconds = this.serializedObject.FindProperty("stopIfStuckForSeconds");
            _stopTimeFrame = this.serializedObject.FindProperty("stopTimeFrame");
            _averageActualVelocity = this.serializedObject.FindProperty("averageActualVelocity");
        }
    }
}
