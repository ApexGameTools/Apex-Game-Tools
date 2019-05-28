/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(bool))]
    public sealed class BoolField : EditorFieldBase<bool>
    {
        public BoolField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.Toggle(_label, _curValue, EditorStyles.toggle);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}