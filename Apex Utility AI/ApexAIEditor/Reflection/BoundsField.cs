/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(Bounds))]
    public sealed class BoundsField : EditorFieldBase<Bounds>
    {
        private Vector3 _center = Vector3.zero;
        private Vector3 _size = Vector3.zero;

        public BoundsField(MemberData data, object owner)
            : base(data, owner)
        {
            _center = _curValue.center;
            _size = _curValue.size;
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            EditorGUILayout.LabelField(_label, EditorStyles.label);
            EditorGUI.indentLevel += 1;

            _center = EditorGUILayout.Vector3Field("Center", _center);
            _size = EditorGUILayout.Vector3Field("Size", _size);

            EditorGUI.indentLevel -= 1;

            var val = new Bounds(_center, _size);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}