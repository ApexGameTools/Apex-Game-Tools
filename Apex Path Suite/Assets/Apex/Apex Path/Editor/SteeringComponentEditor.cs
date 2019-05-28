/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using UnityEditor;
    using UnityEngine;

    public abstract class SteeringComponentEditor : Editor
    {
        private SerializedProperty _weight;
        private SerializedProperty _priority;

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
            }
            
            EditorGUILayout.PropertyField(_weight);
        }

        protected virtual void OnEnable()
        {
            _weight = this.serializedObject.FindProperty("weight");
            _priority = this.serializedObject.FindProperty("priority");
        }
    }
}
