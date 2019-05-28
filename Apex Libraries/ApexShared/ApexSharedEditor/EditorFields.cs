namespace Apex.Editor
{
    using UnityEditor;
    using UnityEngine;

    public static class EditorFields
    {
        private static readonly GUIContent _emptyContent = new GUIContent();

        public static float FloatField(this MonoBehaviour target, float current, GUIContent label, params GUILayoutOption[] options)
        {
            var tmp = EditorGUILayout.FloatField(label ?? _emptyContent, current, options);
            if (tmp != current)
            {
                EditorUtility.SetDirty(target);
            }

            return tmp;
        }

        public static bool ToggleField(this MonoBehaviour target, bool current, GUIContent label, params GUILayoutOption[] options)
        {
            var tmp = EditorGUILayout.Toggle(label ?? _emptyContent, current, options);
            if (tmp != current)
            {
                EditorUtility.SetDirty(target);
            }

            return tmp;
        }

        public static bool ToggleLeftField(this MonoBehaviour target, bool current, GUIContent label, params GUILayoutOption[] options)
        {
            var tmp = EditorGUILayout.ToggleLeft(label ?? _emptyContent, current, options);
            if (tmp != current)
            {
                EditorUtility.SetDirty(target);
            }

            return tmp;
        }
    }
}
