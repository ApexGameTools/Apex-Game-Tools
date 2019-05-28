/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using System.Collections.Generic;
    using Apex.Steering;
    using UnityEditor;

    [CustomEditor(typeof(SpeedComponent), true), CanEditMultipleObjects]
    public class DefaultSpeedComponentEditor : SpeedComponentEditor
    {
        private List<SerializedProperty> _props;

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorGUILayout.Separator();
            foreach (var p in _props)
            {
                EditorGUILayout.PropertyField(p);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _props = this.GetProperties();
        }
    }
}
