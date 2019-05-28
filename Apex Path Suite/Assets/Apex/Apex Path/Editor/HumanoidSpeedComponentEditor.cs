namespace Apex.Editor
{
    using Apex.Steering.Components;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(HumanoidSpeedComponent), false), CanEditMultipleObjects]
    public class HumanoidSpeedComponentEditor : DefaultSpeedComponentEditor
    {
        private float _customSpeed;

        protected override void CreateUI()
        {
            base.CreateUI();

            if (Application.isPlaying)
            {
                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                _customSpeed = EditorGUILayout.FloatField(_customSpeed, GUILayout.Width(100));
                if (GUILayout.Button("Test Custom Speed"))
                {
                    var targets = this.targets;
                    var count = targets.Length;
                    for (int i = 0; i < count; i++)
                    {
                        var hs = targets[i] as HumanoidSpeedComponent;
                        hs.SetPreferredSpeed(_customSpeed);
                        _customSpeed = Mathf.Min(_customSpeed, hs.maximumSpeed);
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.hideMaxSpeed = true;
        }
    }
}
