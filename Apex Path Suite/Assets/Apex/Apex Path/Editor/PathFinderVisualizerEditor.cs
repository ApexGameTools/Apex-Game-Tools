/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using Debugging;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PathFinderVisualizer), false)]
    public class PathFinderVisualizerEditor : Editor
    {
        private SerializedProperty _engineType;
        private SerializedProperty _moveCost;
        private SerializedProperty _initialHeapSize;
        private SerializedProperty _unit;
        private SerializedProperty _costInfo;
        private SerializedProperty _showInstructions;
        private SerializedProperty _stepByStep;

        public override void OnInspectorGUI()
        {
            var resetRequired = false;

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            var oldUnit = _unit.objectReferenceValue;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_unit);
            EditorGUILayout.PropertyField(_engineType);
            EditorGUILayout.PropertyField(_moveCost);
            resetRequired = EditorGUI.EndChangeCheck();

            EditorGUILayout.PropertyField(_costInfo);
            GUI.enabled = !EditorApplication.isPlaying;
            EditorGUILayout.PropertyField(_initialHeapSize);
            GUI.enabled = true;
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_stepByStep);
            EditorGUILayout.PropertyField(_showInstructions);
            this.serializedObject.ApplyModifiedProperties();

            if (resetRequired)
            {
                if (!object.ReferenceEquals(oldUnit, _unit.objectReferenceValue))
                {
                    QuickStarts.NavigatingUnit(_unit.objectReferenceValue as GameObject, false);
                }

                if (EditorApplication.isPlaying)
                {
                    ((PathFinderVisualizer)this.target).Reset();
                }
            }
        }

        private void OnEnable()
        {
            _unit = this.serializedObject.FindProperty("unit");
            _engineType = this.serializedObject.FindProperty("engineType");
            _moveCost = this.serializedObject.FindProperty("moveCost");
            _initialHeapSize = this.serializedObject.FindProperty("initialHeapSize");
            _costInfo = this.serializedObject.FindProperty("costInfo");
            _showInstructions = this.serializedObject.FindProperty("showInstructions");
            _stepByStep = this.serializedObject.FindProperty("stepByStep");
        }
    }
}
