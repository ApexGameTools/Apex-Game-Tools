/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(Rect))]
    public sealed class RectField : EditorFieldBase<Rect>
    {
        public RectField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.RectField(_label, _curValue);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}