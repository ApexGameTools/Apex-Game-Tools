namespace Apex.Steering.Editor
{
    using Apex.Steering.VectorFields;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(VectorFieldOptions))]
    public class VectorFieldOptionsPropertyDrawer : PropertyDrawer
    {
        private const float height = 16f;
        private readonly float heightWithPadding = height + 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float offset = 0f;
            if (!string.IsNullOrEmpty(label.text))
            {
                EditorGUI.LabelField(position, label);
                offset = 18f;
            }

            float extraOffset = 0f;

            EditorGUI.BeginProperty(position, GUIContent.none, property);
            position.y += offset;

            var typeProperty = property.FindPropertyRelative("vectorFieldType");
            var rect = new Rect(position.x, position.y, position.width, height);
            EditorGUI.PropertyField(rect, typeProperty);

            EditorGUI.indentLevel++;

            var builtInContainment = property.FindPropertyRelative("builtInContainment");
            extraOffset = 2f;
            rect.y = position.y + extraOffset + heightWithPadding;
            EditorGUI.PropertyField(rect, builtInContainment);

            extraOffset += 5f;
            rect.y = position.y + extraOffset + (heightWithPadding * 2f);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("updateInterval"));

            rect.y = position.y + extraOffset + (heightWithPadding * 3f);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("obstacleStrengthFactor"));

            int type = typeProperty.intValue;
            if (type == (int)VectorFieldType.ProgressiveField || type == (int)VectorFieldType.CrossGridField)
            {
                extraOffset += 5f;
                rect.y = position.y + extraOffset + (heightWithPadding * 4f);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("expectedGroupGrowthFactor"));

                rect.y = position.y + extraOffset + (heightWithPadding * 5f);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("boundsPadding"));

                rect.y = position.y + extraOffset + (heightWithPadding * 6f);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("paddingIncrease"));

                rect.y = position.y + extraOffset + (heightWithPadding * 7f);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("maxExtraPadding"));

                rect.y = position.y + extraOffset + (heightWithPadding * 8f);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("boundsRecalculateThreshold"));
            }
            else if (type == (int)VectorFieldType.FunnelField)
            {
                extraOffset += 5f;
                rect.y = position.y + extraOffset + (heightWithPadding * 4);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("funnelWidth"));
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float startY = 12f;

            var type = property.FindPropertyRelative("vectorFieldType").intValue;
            if (type == (int)VectorFieldType.ProgressiveField || type == (int)VectorFieldType.CrossGridField)
            {
                if (string.IsNullOrEmpty(label.text))
                {
                    return startY + (heightWithPadding * 9f);
                }

                return startY + (heightWithPadding * 10f);
            }
            else if (type == (int)VectorFieldType.FunnelField)
            {
                if (string.IsNullOrEmpty(label.text))
                {
                    return startY + (heightWithPadding * 5f);
                }

                return startY + (heightWithPadding * 6f);
            }

            if (string.IsNullOrEmpty(label.text))
            {
                return startY + (heightWithPadding * 4f);
            }

            return startY + (heightWithPadding * 5f);
        }
    }
}