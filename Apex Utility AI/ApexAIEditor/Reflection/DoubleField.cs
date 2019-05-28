/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using UnityEditor;

    [TypesHandled(typeof(double))]
    public sealed class DoubleField : EditorFieldBase<double>
    {
        public DoubleField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = (double)EditorGUILayout.FloatField(_label, Convert.ToSingle(_curValue), EditorStyles.numberField);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}