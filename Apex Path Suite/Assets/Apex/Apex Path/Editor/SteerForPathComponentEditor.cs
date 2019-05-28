namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SteerForPathComponent), false), CanEditMultipleObjects]
    public class SteerForPathComponentEditor : ArrivalBaseEditor
    {
        private SerializedProperty _strictPathFollowing;

        protected override void CreateUI()
        {
            base.CreateUI();

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_strictPathFollowing);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _strictPathFollowing = this.serializedObject.FindProperty("strictPathFollowing");
        }
    }
}