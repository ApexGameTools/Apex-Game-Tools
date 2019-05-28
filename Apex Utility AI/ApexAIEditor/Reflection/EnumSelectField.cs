/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using UnityEditor;

    public sealed class EnumSelectField : EditorFieldBase<Enum>
    {
        public EnumSelectField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.EnumPopup(_label, _curValue);
            if (!val.Equals(_curValue))
            {
                UpdateValue(val, state);
            }
        }
    }
}