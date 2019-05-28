/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using Apex.Steering;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(HeightNavigationCapabilities))]
    public class HeightNavigationCapabilitiesPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float offset = 0f;
            if (!string.IsNullOrEmpty(label.text))
            {
                EditorGUI.LabelField(position, label);
                offset = 18f;
            }

            EditorGUI.BeginProperty(position, GUIContent.none, property);

            position.y += offset;
            EditorGUI.indentLevel++;

            var slopeRect = new Rect(position.x, position.y, position.width, 16f);
            var scaleRect = new Rect(position.x, position.y + 18f, position.width, 16f);
            var dropRect = new Rect(position.x, position.y + 36f, position.width, 16f);

            EditorGUI.PropertyField(slopeRect, property.FindPropertyRelative("maxSlopeAngle"), new GUIContent("Max Slope Angle", "The maximum angle at which a unit can walk."));
            EditorGUI.PropertyField(scaleRect, property.FindPropertyRelative("maxClimbHeight"), new GUIContent("Max Climb Height", "The maximum height that the unit can scale, i.e. walk onto even if it is a vertical move. Stairs for instance."));
            EditorGUI.PropertyField(dropRect, property.FindPropertyRelative("maxDropHeight"), new GUIContent("Max Drop Height", "The maximum height from which a unit can drop down to the ground below."));

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(label.text))
            {
                return 54f;
            }

            return 70f;
        }
    }
}
