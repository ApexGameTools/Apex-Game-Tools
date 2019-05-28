/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using UnityEditor;

    public sealed class EnumMaskField : EditorFieldBase<Enum>
    {
        public EnumMaskField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.EnumMaskField(_label, _curValue);
            if (!val.Equals(_curValue))
            {
                UpdateValue(val, state);
            }
        }
    }
}