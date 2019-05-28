/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(ushort))]
    public sealed class UShortField : EditorFieldBase<ushort>
    {
        public UShortField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = (ushort)EditorGUILayout.IntSlider(_label, _curValue, ushort.MinValue, ushort.MaxValue);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}