namespace Apex.AI.Editor.Reflection
{
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(AnimationCurve))]
    public sealed class AnimationCurveField : EditorFieldBase<AnimationCurve>
    {
        public AnimationCurveField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public override void RenderField(AIInspectorState state)
        {
            var val = EditorGUILayout.CurveField(_label, _curValue);
            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}