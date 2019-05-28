/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(string))]
    public sealed class StringField : EditorFieldBase<string>
    {
        public StringField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.TextField(_label, _curValue, EditorStyles.textField);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}