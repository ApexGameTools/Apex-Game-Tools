/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(Vector4))]
    public sealed class Vector4Field : EditorFieldBase<Vector4>
    {
        private static readonly GUIContent[] _labelsArr = new GUIContent[]
        {
            new GUIContent("X"),
            new GUIContent("Y"),
            new GUIContent("Z"),
            new GUIContent("W")
        };

        private float[] _vector4Arr = new float[4];

        public Vector4Field(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            // solution found in EditorGUILayout.Vector4Field in UnityEditor namespace (decompiled)
            var controlRect = EditorGUILayout.GetControlRect(true, 32f, EditorStyles.numberField);
            
            _vector4Arr[0] = _curValue.x;
            _vector4Arr[1] = _curValue.y;
            _vector4Arr[2] = _curValue.z;
            _vector4Arr[3] = _curValue.w;

            controlRect.height = 16f;
            GUI.Label(EditorGUI.IndentedRect(controlRect), _label, EditorStyles.label);
            controlRect.y += 16f;

            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel += 1;
            EditorGUI.MultiFloatField(controlRect, _labelsArr, _vector4Arr);
            EditorGUI.indentLevel -= 1;

            if (EditorGUI.EndChangeCheck())
            {
                _curValue.x = _vector4Arr[0];
                _curValue.y = _vector4Arr[1];
                _curValue.z = _vector4Arr[2];
                _curValue.w = _vector4Arr[3];

                UpdateValue(_curValue, state);
            }
        }
    }
}