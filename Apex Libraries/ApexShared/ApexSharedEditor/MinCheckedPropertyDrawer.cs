namespace Apex.Editor
{
    using Apex.Utilities;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(MinCheckAttribute))]
    public class MinCheckedPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attrib = this.attribute as MinCheckAttribute;

            //For some reason using the passed in label directly results in other properties that have no tooltip to inherit the last tooltip set.
            label = new GUIContent(label.text, label.tooltip);

            if (!string.IsNullOrEmpty(attrib.label))
            {
                label.text = attrib.label;                
            }

            if (!string.IsNullOrEmpty(attrib.tooltip))
            {
                label.tooltip = attrib.tooltip;
            }

            EditorGUI.PropertyField(position, property, label);

            if (property.propertyType == SerializedPropertyType.Float)
            {
                var val = property.floatValue;
                if (val < attrib.min)
                {
                    property.floatValue = attrib.min;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                var val = property.intValue;
                if (val < attrib.min)
                {
                    property.intValue = (int)attrib.min;
                }
            }
            else
            {
                EditorGUI.LabelField(position, "The min check attribute is only valid on int or float fields.");
            }
        }
    }
}
