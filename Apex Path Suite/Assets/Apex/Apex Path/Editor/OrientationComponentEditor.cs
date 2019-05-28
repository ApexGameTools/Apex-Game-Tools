/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;
    using UnityEngine;

    public abstract class OrientationComponentEditor : Editor
    {
        private SerializedProperty _priority;
        private SerializedProperty _slowingDistance;
        private SerializedProperty _slowingAlgorithm;

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            CreateUI();

            this.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void CreateUI()
        {
            EditorGUILayout.Separator();

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Priority", _priority.intValue.ToString());
            }
            else
            {
                EditorGUILayout.PropertyField(_priority);
                EditorGUILayout.Separator();
            }
                        
            EditorGUILayout.PropertyField(_slowingDistance);
            EditorGUILayout.PropertyField(_slowingAlgorithm);
        }

        protected virtual void OnEnable()
        {
            _priority = this.serializedObject.FindProperty("priority");
            _slowingDistance = this.serializedObject.FindProperty("slowingDistance");
            _slowingAlgorithm = this.serializedObject.FindProperty("slowingAlgorithm");
        }
    }
}
