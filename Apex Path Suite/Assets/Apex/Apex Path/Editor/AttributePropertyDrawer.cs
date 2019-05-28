namespace Apex.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Apex.Common;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(AttributePropertyAttribute))]
    public class AttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!AttributesMaster.attributesEnabled)
            {
                EditorGUI.HelpBox(position, "To enable attribute specific behaviours, create an entity attribute enum and decorate it with the EntityAttributesEnum.", MessageType.Info);
                return;
            }

            var attrib = this.attribute as AttributePropertyAttribute;

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

            EditorUtilitiesInternal.EnumToIntField(position, property, AttributesMaster.attributesEnumType, label);
        }
    }
}
