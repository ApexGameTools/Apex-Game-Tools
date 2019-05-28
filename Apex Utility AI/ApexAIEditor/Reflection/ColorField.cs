/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(Color))]
    public sealed class ColorField : EditorFieldBase<Color>
    {
        public ColorField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.ColorField(_label, _curValue); 
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}