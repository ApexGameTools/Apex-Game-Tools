namespace Apex.Editor
{
    using System.Collections.Generic;
    using Apex.LoadBalancing;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(LoadBalancerComponent), false)]
    public class LoadBalancerComponentEditor : Editor
    {
        private static Dictionary<string, bool> _foldedOut = new Dictionary<string, bool>();
        private SerializedProperty _mashallerMaxMillisecondPerFrame;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("These settings cannot be edited in play mode.", MessageType.Info);
                return;
            }

            var lb = this.target as LoadBalancerComponent;

            EditorGUILayout.Separator();
            EditorGUI.indentLevel += 1;
            foreach (var cfg in lb.configurations)
            {
                bool foldOut = false;
                _foldedOut.TryGetValue(cfg.targetLoadBalancer, out foldOut);
                _foldedOut[cfg.targetLoadBalancer] = EditorGUILayout.Foldout(foldOut, cfg.targetLoadBalancer.ExpandFromPascal());
                if (_foldedOut[cfg.targetLoadBalancer])
                {
                    if (DrawConfigEditor(cfg))
                    {
                        EditorUtility.SetDirty(lb);
                    }
                }
            }

            EditorUtilities.Section("Marshaller");
            this.serializedObject.Update();
            EditorGUILayout.PropertyField(_mashallerMaxMillisecondPerFrame, new GUIContent("Max Milliseconds Per Frame", "The maximum number of milliseconds per frame to allow for marshalling calls onto the main thread."));
            this.serializedObject.ApplyModifiedProperties();
        }

        private bool DrawConfigEditor(LoadBalancerConfig cfg)
        {
            bool changed = false;
            EditorGUILayout.BeginVertical("Box");

            var interval = EditorGUILayout.FloatField("Update Interval", cfg.updateInterval);
            if (cfg.updateInterval != interval)
            {
                cfg.updateInterval = interval;
                changed = true;
            }

            var auto = EditorGUILayout.Toggle("Auto Adjust", cfg.autoAdjust);
            if (cfg.autoAdjust != auto)
            {
                cfg.autoAdjust = auto;
                changed = true;
            }

            if (!auto)
            {
                var maxUpdates = EditorGUILayout.IntField("Max Updates Per Frame", cfg.maxUpdatesPerFrame);
                if (cfg.maxUpdatesPerFrame != maxUpdates)
                {
                    cfg.maxUpdatesPerFrame = maxUpdates;
                    changed = true;
                }

                var maxUpdateTime = EditorGUILayout.IntField("Max Update Time In Milliseconds Per Update", cfg.maxUpdateTimeInMillisecondsPerUpdate);
                if (cfg.maxUpdateTimeInMillisecondsPerUpdate != maxUpdateTime)
                {
                    cfg.maxUpdateTimeInMillisecondsPerUpdate = maxUpdateTime;
                    changed = true;
                }
            }

            EditorGUILayout.EndVertical();
            return changed;
        }

        private void OnEnable()
        {
            _mashallerMaxMillisecondPerFrame = this.serializedObject.FindProperty("_mashallerMaxMillisecondPerFrame");
        }
    }
}
