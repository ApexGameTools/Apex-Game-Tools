namespace Apex.Editor
{
    using Apex.Debugging;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(LoadBalancerPerformanceVisualizer), false)]
    public class LoadBalancerPerformanceVisualizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("This info is only available in play mode.", MessageType.Info);
                return;
            }

            var lb = this.target as LoadBalancerPerformanceVisualizer;

            foreach (var d in lb.data)
            {
                EditorGUILayout.LabelField(string.Concat(d.loadBalancerName.ExpandFromPascal(), " (", d.itemsCount, " items)"));
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Frame Updated Items Count", d.frameUpdatedItemsCount.ToString());
                EditorGUILayout.LabelField("Frame Milliseconds Used", d.frameUpdateMillisecondsUsed.ToString());
                EditorGUILayout.LabelField("Frame Overdue Average", d.frameUpdatesOverdueAverage.ToString("0.###"));

                EditorGUILayout.LabelField("Average Updated Items Count", d.averageUpdatedItemsCount.ToString("0"));
                EditorGUILayout.LabelField("Average Milliseconds Used", d.averageUpdateMillisecondsUsed.ToString("0.###"));
                EditorGUILayout.LabelField("Average Overdue Average", d.averageUpdatesOverdueAverage.ToString("0.###"));
                EditorGUI.indentLevel--;
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }
    }
}
