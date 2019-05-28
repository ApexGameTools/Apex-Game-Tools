/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(short))]
    public sealed class ShortField : EditorFieldBase<short>
    {
        public ShortField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = (short)EditorGUILayout.IntSlider(_label, _curValue, short.MinValue, short.MaxValue);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}