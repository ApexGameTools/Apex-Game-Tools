/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;

    [CustomEditor(typeof(SteerForFormationComponent), true), CanEditMultipleObjects]
    public class DefaultSteerForFormationEditor : ArrivalBaseEditor
    {
        private SerializedProperty _debugDraw;
        private SerializedProperty _sampledCellCount;
        private SerializedProperty _samplingUpdateInterval;
        private SerializedProperty _maxFormationRadius;
        private SerializedProperty _dropFormationOnArrival;

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorUtilities.Section("Formation");
            
            EditorGUILayout.PropertyField(_samplingUpdateInterval);
            EditorGUILayout.PropertyField(_sampledCellCount);
            EditorGUILayout.PropertyField(_maxFormationRadius);
            EditorGUILayout.PropertyField(_dropFormationOnArrival);

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_debugDraw);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _debugDraw = this.serializedObject.FindProperty("debugDraw");
            _sampledCellCount = this.serializedObject.FindProperty("sampledCellCount");
            _samplingUpdateInterval = this.serializedObject.FindProperty("samplingUpdateInterval");
            _maxFormationRadius = this.serializedObject.FindProperty("maxFormationRadius");
            _dropFormationOnArrival = this.serializedObject.FindProperty("dropFormationOnArrival");
        }
    }
}