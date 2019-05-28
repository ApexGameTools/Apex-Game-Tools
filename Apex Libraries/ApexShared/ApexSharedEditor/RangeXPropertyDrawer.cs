/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using Apex.Utilities;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(RangeXAttribute))]
    public class RangeXPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangeXAttribute rangeAttribute = (RangeXAttribute)this.attribute;

            //For some reason using the passed in label directly results in other properties that have no tooltip to inherit the last tooltip set.
            label = new GUIContent(label.text, label.tooltip);

            if (!string.IsNullOrEmpty(rangeAttribute.label))
            {
                label.text = rangeAttribute.label;
            }

            if (!string.IsNullOrEmpty(rangeAttribute.tooltip))
            {
                label.tooltip = rangeAttribute.tooltip;
            }

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.Slider(position, property, rangeAttribute.min, rangeAttribute.max, label);
            }
            else if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
            }
            else
            {
                EditorGUI.IntSlider(position, property, (int)rangeAttribute.min, (int)rangeAttribute.max, label);
            }
        }
    }
}
