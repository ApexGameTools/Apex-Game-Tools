/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;

    [TypesHandled(typeof(byte))]
    public sealed class ByteField : EditorFieldBase<byte>
    {
        public ByteField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = (byte)EditorGUILayout.IntSlider(_label, _curValue, byte.MinValue, byte.MaxValue);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}