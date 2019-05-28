/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(Vector3))]
    public sealed class Vector3Field : EditorFieldBase<Vector3>
    {
        public Vector3Field(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.Vector3Field(_label, _curValue);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}