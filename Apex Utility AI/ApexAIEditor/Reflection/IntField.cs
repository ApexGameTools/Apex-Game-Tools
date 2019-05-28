/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(int))]
    public sealed class IntField : EditorFieldBase<int>
    {
        public IntField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.IntField(_label, _curValue, EditorStyles.numberField);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}