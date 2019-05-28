/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(Vector2))]
    public sealed class Vector2Field : EditorFieldBase<Vector2>
    {
        public Vector2Field(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.Vector2Field(_label, _curValue);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}