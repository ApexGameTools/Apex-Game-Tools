/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(char))]
    public sealed class CharField : EditorFieldBase<char>
    {
        public CharField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var stringVal = EditorGUILayout.TextField(_label, _curValue.ToString(), EditorStyles.textField);
            char val = stringVal.Length > 0 ? stringVal[0] : '\0';
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}