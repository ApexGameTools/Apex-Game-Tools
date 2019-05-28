/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using UnityEditor;
    using UnityEngine;

    public abstract class SpeedComponentEditor : Editor
    {
        private SerializedProperty _maxAcceleration;
        private SerializedProperty _maxDeceleration;
        private SerializedProperty _maxAngularAcceleration;
        private SerializedProperty _maxAngularSpeed;
        private SerializedProperty _minimumSpeed;
        private SerializedProperty _maximumSpeed;

        protected bool hideMaxSpeed
        {
            get;
            set;
        }

        protected bool hideMinSpeed
        {
            get;
            set;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            CreateUI();

            this.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void CreateUI()
        {
            EditorUtilities.Section("Rotation");
            EditorGUILayout.PropertyField(_maxAngularAcceleration);
            EditorGUILayout.PropertyField(_maxAngularSpeed);

            EditorUtilities.Section("Movement");
            EditorGUILayout.PropertyField(_maxAcceleration);
            EditorGUILayout.PropertyField(_maxDeceleration);

            if (!this.hideMinSpeed)
            {
                EditorGUILayout.PropertyField(_minimumSpeed);
            }

            if (!this.hideMaxSpeed)
            {
                EditorGUILayout.PropertyField(_maximumSpeed);
            }
        }

        protected virtual void OnEnable()
        {
            _maxAcceleration = this.serializedObject.FindProperty("_maxAcceleration");
            _maxDeceleration = this.serializedObject.FindProperty("_maxDeceleration");
            _maxAngularAcceleration = this.serializedObject.FindProperty("_maxAngularAcceleration");
            _maxAngularSpeed = this.serializedObject.FindProperty("_maxAngularSpeed");
            _minimumSpeed = this.serializedObject.FindProperty("_minimumSpeed");
            _maximumSpeed = this.serializedObject.FindProperty("_maximumSpeed");
        }
    }
}
