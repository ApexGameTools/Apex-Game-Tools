/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(float))]
    public sealed class FloatField : EditorFieldBase<float>
    {
        public FloatField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.FloatField(_label, _curValue, EditorStyles.numberField);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}