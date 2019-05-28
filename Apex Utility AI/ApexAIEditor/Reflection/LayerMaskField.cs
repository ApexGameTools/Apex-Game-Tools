/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(LayerMask))]
    public sealed class LayerMaskField : EditorFieldBase<LayerMask>
    {
        public LayerMaskField(MemberData data, object owner)
            : base(data, owner)
        {
        }

        public sealed override void RenderField(AIInspectorState state)
        {
            var val = _curValue;

            var layers = UnityEditorInternal.InternalEditorUtility.layers;
            var layerNumbers = new int[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                layerNumbers[i] = LayerMask.NameToLayer(layers[i]);
            }

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Length; i++)
            {
                if (((1 << layerNumbers[i]) & val.value) > 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(_label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i < layerNumbers.Length; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }

                val.value = mask;
            }

            if (val != _curValue)
            {
                UpdateValue(val, state);
            }
        }
    }
}